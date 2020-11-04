﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoKallSMC.Communication
{
    public static class CommunicationCommands
    {
        public static readonly string MCCConnectionStatusCheckCmd = "<MCCS>"; //MCC Connection status
        public static readonly string SBCConnectionResponseCmd = "<SMCR>"; //SMC Connection response
        public static readonly string MCCConnection = "<SMCC>"; //SMC Connection 
        public static readonly string SMCPODDEPLOY = "<P{0}D>"; //PULSE Oximeter Pulse Deploymenent
        public static readonly string SMCPDDEPLOYSTATUS = "<P{0}DG>"; //PULSE Oximeter Pulse Deploymenent
        public static readonly string SMCRESULT = "<P{0}DR>{1}"; //PULSE Oximeter result
        public static readonly string SMCUSAGEDONE = "<P{0}DS>";
        public static readonly string SMCPULSEOXIMETERSTART = "<PULSESTART>";
        public static readonly string PUSLEOXIMETERRESULT = "PULSERES>SP:{0}>PR:{1}>DT>{2}>";
        public static readonly string PUSLEOXIMETERCONNECTIONMSG = "PULSESTATUS>{0}>{1}>";

        public static readonly string GLUCORESULTCMD = "<GLUCMD>";
        public static readonly string GLUCORESULTRES = "GLUCMDRES>V:{0}>U:{1}>T:{2}>M:{3}>D:{4}>T>{5}>";
        public static readonly string GLUCORESULTRESSTATUS = "GLUCMDRESSTATUS>M:{0}";
        public static readonly string THERMORESULT = "THERMORES>R:{0}>M:{1}>S:{2}>{3}>";
        public static readonly string THERMORESULTRESSTATUS = "THERMOCON>{0}";
        public static readonly string THERMORESULTCMD = "<THERMOCMD>";
        public static readonly string THERMORCONNECTIONSTATUS = "THERMOCON>{0}>";
        public static readonly string THERMORError = "THERMOERROR>{0}>";
        public static readonly string THERMOnotpaired = "THERMOnotpaired>{0}>";

        public static readonly string BPCMD = "<BPCMD>";
        public static readonly string BPCONCMD = "<BPCONCMD>";
        public static readonly string BPCONSTATUS = "BPCONN>M:{0}";
        public static readonly string BPCONNECTIONTIME = "BPCONECTED>M:{0}>T:{1}";
        public static readonly string BPRESULT = "BPRES>D:{0}>S:{1}>P:{2}>DT:{3}>T:{4}>";



        public static readonly string DATAACQUISTIONAPPCONNECTION = "<APPC>"; //dataacquistion Connection
        public static readonly string DATAACQSTATUS = "<APPS>"; //dataacquistion Status
        public static readonly string DERMASACOPE = "DER>SH:{0}>H:{1}>W:{2}>X:{3}>Y:{4}>";

        public static readonly string OTOSCOPE = "OTO>SH:{0}>H:{1}>W:{2}>X:{3}>Y:{4}>";

        public static readonly string TAKEPIC = "<PIC>";
        public static readonly string STARTDERMO = "<startdermo>";
        public static readonly string STOPDERMO = "<stopdermo>";
        public static readonly string STARTOTOSCOPE = "<startoto>";
        public static readonly string STOPOTOSCOPE = "<stopoto>";
        public static readonly string STARTSTCHEST = "<startstchecst>";
        public static readonly string STCHESTRESPONSE = "streadey>{0}";
        public static readonly string STMSG = "stmsg>{0}";

        public static readonly string STPIC = "STPIC>{0}>";
        public static readonly string DERPIC = "DRPIC>{0}>";
        public static readonly string MIROSCOPEPIC = "MRPIC>{0}>";
        public static readonly string OTOSAVEIMAGE = "<otosaveimage>";
        public static readonly string DERSAVEIMAGE = "<dersaveimage>";
        public static readonly string NotifySAVEDIMAGE = "<imagesaved>";

        public static readonly string SpirometerFVCdata = "SPIROFVC>{0}>";
        public static readonly string SpirometerFVC = "SPIROVC>{0}>";

        public static readonly string StartSpiroFVC = "<startspirofvc>";
        public static readonly string StartSpiroVC = "<startspirovc>";
        public static readonly string StopSpiro = "<stopspiro>";
        public static readonly string SBCShutdown = "<sbcshutdown>";
        public static readonly string SBCStart = "<sbcstart>";

    }
}
