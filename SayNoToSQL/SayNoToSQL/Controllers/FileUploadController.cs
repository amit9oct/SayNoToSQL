using Microsoft.ProgramSynthesis.AST;
using SayNoToSQL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SayNoToSQL.Controllers
{
    public class FileUploadController : Controller {
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
        public ActionResult Upload(HttpPostedFileBase input_file, HttpPostedFileBase output_file)
        {
            if (UploadFile(input_file) && UploadFile(output_file))
            {
                ViewBag.Message = "Files Uploaded Successfully!!";
                ViewBag.InputFile = Path.GetFileName(input_file.FileName);
                ViewBag.OutputFile = Path.GetFileName(output_file.FileName);
                var inputTable = OpenFile(Path.GetFileName(input_file.FileName));
                var outputTable = OpenFile(Path.GetFileName(output_file.FileName),inputTable);
                try
                {
                    if (SNTSBackend.Learner.GrammarNotCompiled) {
                        Directory.SetCurrentDirectory(Server.MapPath("~/bin/"));
                        SNTSBackend.Learner.Instance.SetGrammar(Server.MapPath("~/App_Data/Grammar/SQL.grammar"));
                    }
                    var allProgramNodes = SNTSBackend.Learner.Instance.LearnSQLAll(inputTable, outputTable);
                    ViewBag.AllProgramNodes = allProgramNodes;
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
