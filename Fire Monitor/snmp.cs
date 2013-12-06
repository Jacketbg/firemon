using System;
using adventnet.snmp.snmp2;
using System.Net;
using System.Windows.Forms;


namespace Jacket
{
    namespace Classes
    {
        class mysnmp
        {
            string error;
            public string getLastErr()
            {
                return error;
            }
            public string get(string ip, string community, string oid, int timeout, int retry)
            {
                IPAddress ipaddr = IPAddress.Parse(ip);
                SnmpAPI api = new SnmpAPI();
                api.Debug = false;
                SnmpSession session = new SnmpSession(api);

                try
                {
                    session.Open();
                }
                catch (SnmpException e)
                {
                    error = "Error opening socket: " + e;
                    return "false";
                }

                SnmpPDU pdu = new SnmpPDU();

                UDPProtocolOptions option = new UDPProtocolOptions(ipaddr, 161);
                pdu.ProtocolOptions = option;
                pdu.Community = community;
                pdu.Timeout = timeout;
                pdu.Retries = retry;
                pdu.Command = SnmpAPI.GET_REQ_MSG;

                SnmpOID send_oid = new SnmpOID(oid);
                pdu.AddNull(send_oid);

                SnmpPDU result = null;
                try
                {
                    result = session.SyncSend(pdu);
                }
                catch (SnmpException e)
                {
                    error = "Error sending SNMP request: " + e;
                    return "false";
                }

                if (result == null)
                {
                    error = "Request timed out!";
                    return "false";
                }
                else
                {
                    if (result.Errstat == 0)
                    {
                        session.Close();
                        api.Close();
                        return result.GetVariable(0).ToString();
                    }
                    else
                    {
                        session.Close();
                        api.Close();
                        error = result.Error;
                        return "false";
                    }
                }
            }
        }
    }
}