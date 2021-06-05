using System;
using System.Collections.Generic;

namespace AssemblerTwo.Machine.IOBusBridge
{
    // Limitations:
    // Two clients can't use the same port(s)
    // Clients can't use discontinuous ports
    public class DefaultIOBusBridge : IIOBusBridge
    {
        public const int MAX_PORT = 0xFFFF;

        public enum UnconnectedPortBehaviour
        {
            /// <summary>
            /// Write: Fail silently. Read: Return zero.
            /// </summary>
            SilentZero,
            /// <summary>
            /// Write/Read: Throw an <seealso cref="UnconnectedPortException"/>
            /// </summary>
            Excep,
        }

        readonly DefaultIOBusBridgeClient mBridgeClient;

        readonly UnconnectedPortBehaviour mUnconnectedPortBehaviour = UnconnectedPortBehaviour.SilentZero;

        readonly Dictionary<int, IIOBusBridgeClient> mPortToClientMap = new Dictionary<int, IIOBusBridgeClient>();
        readonly Dictionary<IIOBusBridgeClient, int[]> mClientToPortMap = new Dictionary<IIOBusBridgeClient, int[]>();

        int mNextClientDeviceId;
        readonly Dictionary<int, IIOBusBridgeClient> mIdToClientMap = new Dictionary<int, IIOBusBridgeClient>();
        readonly List<int> mOrderedClientIdList = new List<int>();

        public DefaultIOBusBridge(bool bridgeIsClientDevice = true)
        {
            if (bridgeIsClientDevice)
            {
                mBridgeClient = new DefaultIOBusBridgeClient(this);
                Connect(mBridgeClient);
            }
        }

        public DefaultIOBusBridge(bool bridgeIsClientDevice, UnconnectedPortBehaviour unconnectedPortBehaviour) : this(bridgeIsClientDevice)
        {
            mUnconnectedPortBehaviour = unconnectedPortBehaviour;
        }

        public void Connect(IIOBusBridgeClient clientDevice)
        {
            if (mPortToClientMap.ContainsValue(clientDevice))
            {
                throw new ArgumentException($"{nameof(IIOBusBridgeClient)} is already connected to this instance.", nameof(clientDevice));
            }

            var requiredPorts = clientDevice.BusRequiredPorts;
            var baseAddress = clientDevice.BusPreferBaseAddress;
            var canRelocate = clientDevice.BusCanRelocate;

            bool canAllocatePrefered = true;
            for (int x = baseAddress; x < (baseAddress + requiredPorts); x++)
            {
                if (mPortToClientMap.ContainsKey(x))
                {
                    canAllocatePrefered = false;
                    break;
                }
            }

            if (!canRelocate && !canAllocatePrefered)
            {
                throw new ArgumentException($"One or more ports for non-relocatable {nameof(IIOBusBridgeClient)} is already assigned on this instance.", nameof(clientDevice));
            }

            if (!canAllocatePrefered)
            {
                baseAddress = FindUnallocatedRange(requiredPorts);
                if (baseAddress == -1)
                {
                    throw new ArgumentException($"Cannot find unallocated port space for {nameof(IIOBusBridgeClient)} on this instance.", nameof(clientDevice));
                }
                //clientDevice.Relocated(baseAddress); // TODO
            }

            int[] portArray = new int[requiredPorts];
            int portArrayIndex = 0;
            for (int x = baseAddress; x < (baseAddress + requiredPorts); x++)
            {
                CheckPortRange(x);
                mPortToClientMap.Add(x, clientDevice);
                portArray[portArrayIndex] = x;
                portArrayIndex++;
            }
            mClientToPortMap.Add(clientDevice, portArray);

            mIdToClientMap.Add(mNextClientDeviceId, clientDevice);
            mOrderedClientIdList.Add(mNextClientDeviceId);
            mNextClientDeviceId++;
        }

        public void Disconnect(IIOBusBridgeClient clientDevice)
        {
            if (!mPortToClientMap.ContainsValue(clientDevice))
            {
                throw new ArgumentException($"{nameof(IIOBusBridgeClient)} is not connected to this instance.", nameof(clientDevice));
            }

            var portRange = mClientToPortMap[clientDevice];
            for (int x = 0; x < portRange.Length; x++)
            {
                mPortToClientMap.Remove(x);
            }
            mClientToPortMap.Remove(clientDevice);
        }

        public int IOBusRead(int portNumber)
        {
            CheckPortRange(portNumber);

            if (mPortToClientMap.TryGetValue(portNumber, out IIOBusBridgeClient clientDevice))
            {
                return clientDevice.BusBridgeRead(portNumber);
            }
            else
            {
                if (mUnconnectedPortBehaviour == UnconnectedPortBehaviour.Excep)
                {
                    throw new UnconnectedPortException(portNumber);
                }
                else
                {
                    return 0;
                }
            }
        }

        public void IOBusWrite(int portNumber, int value)
        {
            CheckPortRange(portNumber);

            if (mPortToClientMap.TryGetValue(portNumber, out IIOBusBridgeClient clientDevice))
            {
                clientDevice.BusBridgeWrite(portNumber, value);
            }
            else
            {
                if (mUnconnectedPortBehaviour == UnconnectedPortBehaviour.Excep)
                {
                    throw new UnconnectedPortException(portNumber);
                }
                // else fail silently
            }
        }

        private int FindUnallocatedRange(int length)
        {
            for (int baseAddr = 0; baseAddr < MAX_PORT; baseAddr++)
            {
                var isUseableRange = true;
                for (int x = 0; x < length; x++)
                {
                    var checkAddr = baseAddr + x;
                    CheckPortRange(checkAddr);
                    if (mPortToClientMap.ContainsKey(checkAddr))
                    {
                        // TODO: If x > 0, we can alter baseAddr to skip these ports
                        // (currently it will check these slots again on the next outer loop)
                        isUseableRange = false;
                        break;
                    }
                }

                if (isUseableRange)
                {
                    return baseAddr;
                }
            }
            return -1;
        }

        private void CheckPortRange(int portNumber)
        {
            // TODO: Maybe we don't need this function at all
            if (portNumber < 0 || portNumber > MAX_PORT)
            {
                throw new IndexOutOfRangeException($"Port number {portNumber} is invalid (i > 0 && i < {MAX_PORT}).");
            }
        }

        private class DefaultIOBusBridgeClient : IIOBusBridgeClient
        {
            // DOCS:
            // - RESET COMMAND STATE
            //     Write 0
            // - GET DEVICEID(S)
            //     Write 1
            //     Read array length
            //     Read Device Id (for length of array)
            // - GET DEVICE PORT(S)
            //     Write 2
            //     Write DEVICEID
            //     Read array length
            //     Read port number (for length of array)

            public int BusRequiredPorts => 1;

            public int BusPreferBaseAddress => 0;

            public bool BusCanRelocate => false;

            readonly private DefaultIOBusBridge mParent;

            private int mMajorState;
            private int mMinorState;

            private int mSelectedDevice;

            public void Relocated(int newBaseAddress)
            {
                // since BusCanRelocate is false, this should not be called
                throw new NotImplementedException();
            }

            public DefaultIOBusBridgeClient(DefaultIOBusBridge parent)
            {
                mParent = parent;
            }

            public int BusBridgeRead(int portNumber)
            {
                var portOffset = (portNumber - BusPreferBaseAddress);
                if (portOffset != 0)
                {
                    return 0;
                }

                switch (mMajorState)
                {
                    case 0:
                    {
                        return 0;
                    }
                    case 1:
                    {
                        var orderedClients = mParent.mOrderedClientIdList;
                        if (mMinorState == 0)
                        {
                            var len = orderedClients.Count;
                            mMinorState++;
                            return len;
                        }
                        else
                        {
                            if (mMinorState <= orderedClients.Count)
                            {
                                var value = mParent.mOrderedClientIdList[mMinorState - 1];
                                mMinorState++;
                                return value;
                            }
                            else
                            {
                                //mMajorState = 0;
                                //mMinorState = 0;
                                return 0;
                            }
                        }
                    }
                    case 2:
                    {
                        if (mMinorState > 1)
                        {
                            if (mParent.mIdToClientMap.TryGetValue(mSelectedDevice, out IIOBusBridgeClient clientDevice))
                            {
                                var portArray = mParent.mClientToPortMap[clientDevice];
                                if (mMinorState == 2)
                                {
                                    var len = portArray.Length;
                                    mMinorState++;
                                    return len;
                                }
                                else if (mMinorState > 2)
                                {
                                    if (mMinorState <= portArray.Length)
                                    {
                                        var value = portArray[mMinorState - 1];
                                        mMinorState++;
                                        return value;
                                    }
                                    else
                                    {
                                        //mMajorState = 0;
                                        //mMinorState = 0;
                                        return 0;
                                    }
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    default:
                    {
                        throw new NotImplementedException($"{mMajorState}");
                    }
                }
            }

            public void BusBridgeWrite(int portNumber, int value)
            {
                var portOffset = (portNumber - BusPreferBaseAddress);
                if (portOffset != 0)
                {
                    return;
                }

                switch (value)
                {
                    case 0:
                    {
                        mMajorState = 0;
                        mMinorState = 0;
                        break;
                    }
                    case 1:
                    {
                        if (mMajorState == 0)
                        {
                            mMajorState = 1;
                            mMinorState = 0;
                        }
                        break;
                    }
                    case 2:
                    {
                        if (mMajorState == 0)
                        {
                            mMajorState = 2;
                            mMinorState = 1;
                        }
                        else
                        {
                            if (mMinorState == 1)
                            {
                                mSelectedDevice = value;
                                mMinorState = 2;
                            }
                        }
                        break;
                    }
                    default:
                    {
                        throw new NotImplementedException($"{value}");
                    }
                }
            }

        }

        public class UnconnectedPortException : Exception
        {
            // TODO?: Should this include ArgumentException as an innerException?

            public int PortNumber { get; private set; }

            public UnconnectedPortException(int portNumber) : base ($"Port {portNumber} is not connected to any {nameof(IIOBusBridgeClient)} on this instance!")
            {
                PortNumber = portNumber;
            }
        }
    }
}
