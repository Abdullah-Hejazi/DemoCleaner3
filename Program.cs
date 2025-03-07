﻿using DemoCleaner3.ExtClasses;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Text;

namespace DemoCleaner3
{
    static class Program
    {
        enum RunType {
            DEFAULT,
            XML,
            XML_FILE,
            REC
        }

        /// <summary>
        /// The main entry point for the app.
        /// </summary>
        [STAThread]
        static void Main(String[] argg) 
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, arg) => {
                if (arg.Name.StartsWith("LinqBridge")) return Assembly.Load(Properties.Resources.LinqBridge);
                return null;
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            FileInfo demoFile = null;
            RunType runType = RunType.DEFAULT;

            if (argg.Length == 1) {
                demoFile = new FileInfo(argg[0]);
            } else if (argg.Length == 2 && argg[0] == "--xml") {
                demoFile = new FileInfo(argg[1]);
                runType = RunType.XML;
            } else if (argg.Length == 2 && argg[0] == "--xmlfile") {
                demoFile = new FileInfo(argg[1]);
                runType = RunType.XML_FILE;
            } else if ((argg.Length == 3 || argg.Length == 2) && argg[0] == "--rec") {
                demoFile = new FileInfo(argg[1]);
                runType = RunType.REC;
            }
            if (demoFile != null && demoFile.Exists && demoFile.Extension.ToLowerInvariant().StartsWith(".dm_")) {
                Demo demo = null;
                switch (runType) {
                    case RunType.DEFAULT:
                        DemoInfoForm demoInfoForm = new DemoInfoForm();
                        demoInfoForm.demoFile = demoFile;
                        Application.Run(demoInfoForm);
                        break;
                    case RunType.XML:
                        try {
                            demo = Demo.GetDemoFromFileRaw(demoFile);
                            var xmlString = XmlUtils.FriendlyInfoToXmlString(demo.rawInfo.getFriendlyInfo());
                            Console.WriteLine(xmlString);
                        } catch (Exception ex) {
                            Console.WriteLine("Can not parse demo");
                        }
                        break;
                    case RunType.XML_FILE:
                        var name = demoFile.Name.Substring(0, demoFile.Name.Length - demoFile.Extension.Length) + ".xml";
                        var m_exePath = Path.GetDirectoryName(Application.ExecutablePath);
                        var filePath = Path.Combine(m_exePath, name);
                        var fileStream = new FileStream(filePath, FileMode.CreateNew);
                        var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                        try {
                            demo = Demo.GetDemoFromFileRaw(demoFile);
                            var info = demo.rawInfo.getFriendlyInfo();
                            var xmlString = XmlUtils.FriendlyInfoToXmlString(info);
                            streamWriter.Write(xmlString);
                        } catch (Exception ex) {
                            streamWriter.Write("Can not parse demo: " + ex.Message);
                        }
                        streamWriter.Close();
                        fileStream.Close();
                        break;
                    case RunType.REC:
                        try {
                            demo = Demo.GetDemoFromFileRaw(demoFile);
                        } catch (Exception ex) {
                            Console.WriteLine("Can not parse demo");
                        }
                        if (demo != null) {
                            var saver = new RecFileSaver(demo);
                            if (saver.canSave) {
                                try {
                                    if (argg.Length == 3) {
                                        saver.Save(argg[2]);
                                    }
                                    if (argg.Length == 2) {
                                        saver.Save();
                                    }
                                    System.Diagnostics.Debug.WriteLine("rec file saved");
                                } catch (Exception ex) {
                                    Console.WriteLine(ex.Message);
                                }
                            }
                            Console.WriteLine("Can not create rec file for current demo");
                        }
                        break;
                }
            } else {
                Application.Run(new Form1());
            }
        }
    }
}
