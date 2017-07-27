using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Rules;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SayNoToSQL.Controllers
{
    public class RunProgramController : Controller
    {
        // GET: RunProgram
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RunProgram(string encoded, HttpPostedFileBase run_file)
        {
            var pNode = new NonterminalNode(SNTSBackend.Learner.Instance.Grammar.Rules[0] as NonterminalRule);
            if (UploadFile(run_file))
            {
                ViewBag.Message = "Files Uploaded Successfully!!";
                ViewBag.InputFile = "out-"+Path.GetFileName(run_file.FileName);
                var inputTable = OpenFile(Path.GetFileName(run_file.FileName));
                var outputTable = SNTSBackend.Learner.Instance.Invoke(pNode, inputTable);
                SNTSBackend.Parser.DatatableToCSVWriter.Write(outputTable, Path.Combine(Server.MapPath("~/Uploads/"),$"out-{run_file.FileName}"));
            }
            else {
                ViewBag.Message = "Files Not Uploaded!!";
            }
            return View();
        }
        public bool UploadFile(HttpPostedFileBase file)
        {
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
        public DataTable OpenFile(string fileName)
        {
            try
            {
                return SNTSBackend.Parser.CSVToDatatableParser.Parse(Server.MapPath("~/Uploads/" + fileName));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}