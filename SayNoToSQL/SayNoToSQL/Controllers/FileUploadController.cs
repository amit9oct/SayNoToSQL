using Microsoft.ProgramSynthesis.AST;
using SayNoToSQL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;

namespace SayNoToSQL.Controllers
{
    public class ProgramNodeSerializer {
        public ProgramNode ProgramNode { get; set; }
        public string HumanReadable { get; set; }
        public int Idx { get; set;}
        public string Encoded { get {
                return SNTSBackend.Utils.Utils.Base64Encode(HumanReadable);
            }
        }

        public string LargeFile { get; set; }
    }

    public class FileUploadController : Controller {
        public static Dictionary<string, ProgramNode> ProgramNodeDict = new Dictionary<string, ProgramNode>();
        //
        // GET: /FileUpload/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FileUpload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase input_file, HttpPostedFileBase output_file, HttpPostedFileBase large_file)
        {
            if (UploadFile(input_file) && UploadFile(output_file) && UploadFile(large_file))
            {
                ViewBag.Message = "Files Uploaded Successfully!!";
                ViewBag.InputFile = Path.GetFileName(input_file.FileName);
                ViewBag.OutputFile = Path.GetFileName(output_file.FileName);
                var inputTable = OpenFile(Path.GetFileName(input_file.FileName));
                var outputTable = OpenFile(Path.GetFileName(output_file.FileName),inputTable);
                var largeTable = OpenFile(Path.GetFileName(large_file.FileName));
                try
                {
                    if (SNTSBackend.Learner.GrammarNotCompiled)
                    {
                        Directory.SetCurrentDirectory(Server.MapPath("~/bin/"));
                        SNTSBackend.Learner.Instance.SetGrammar(Server.MapPath("~/App_Data/Grammar/SQL.grammar"));
                    }
                    var allProgramNodes = SNTSBackend.Learner.Instance.LearnSQLAll(inputTable, outputTable);
                    var prog = allProgramNodes.First();
                    var ser = prog.PrintAST();
                    var idx = 0;
                    var serializedProgramNodes = new List<ProgramNodeSerializer>();
                    foreach (var pNode in allProgramNodes) {
                        serializedProgramNodes.Add(new ProgramNodeSerializer() {
                            ProgramNode = pNode,
                            HumanReadable = SNTSBackend.Learner.Instance.Query(pNode),
                            Idx = idx,
                            LargeFile = $"out_{idx}_{Path.GetFileName(large_file.FileName)}"
                        });
                        idx++;
                    }
                    
                    foreach (var pNode in serializedProgramNodes)
                    {
                        var perQueryOutputTable = SNTSBackend.Learner.Instance.Invoke(pNode.ProgramNode, largeTable);
                        SNTSBackend.Parser.DatatableToCSVWriter.Write(perQueryOutputTable, Path.Combine(Server.MapPath("~/Uploads/"), pNode.LargeFile));
                    }
                    ViewBag.AllProgramNodes = serializedProgramNodes;
                }
                catch(Exception e)
                {
                    ViewBag.AllProgramNodes = new ProgramNode[] { };
                }   
            }
            else
            {
                ViewBag.Message = "File upload failed!!";
            }
            return View();
        }

        [HttpPost]
        public ActionResult RunProgram(string filename)
        {
            ViewBag.InputFile = Path.GetFileName(filename);
            return View();
        }

        public bool UploadFile(HttpPostedFileBase file) {
            try
            {
                if (file.ContentLength > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    string _path = Path.Combine(Server.MapPath("~/Uploads"), _FileName);
                    file.SaveAs(_path);
                }

                return true;
            }
            catch
            {

                return false;
            }

        }
        public DataTable OpenFile(string fileName) {
            try {
                return SNTSBackend.Parser.CSVToDatatableParser.Parse(Server.MapPath("~/Uploads/" + fileName));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable OpenFile(string fileName, DataTable dataTable)
        {
            try
            {
                return SNTSBackend.Parser.CSVToDatatableParser.Parse(Server.MapPath("~/Uploads/" + fileName), dataTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public ActionResult GetAttachment(string file_name)
        {
            string projectPath = Server.MapPath("~/Uploads/");
            string file = Path.Combine(projectPath, file_name);
            // at this stage file will look something like this 
            // "c:\inetpub\wwwroot\Uploads\Project\foo.pdf". Make sure that
            // this is a valid PDF file and pass it to the File method

            return File(file, "application/vnd.ms-excel", file_name);
        }
    }
}
