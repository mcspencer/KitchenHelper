using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using KitchenHelper.Models;

namespace KitchenHelper.Controllers
{
    public class RecipeController : Controller
    {
        private KitchenHelperDB db = new KitchenHelperDB();

        //
        // GET: /Recipe/

        public ActionResult Index(string sort, string searchQuery)
        {
            ViewBag.SortString = sort;
            ViewBag.SearchString = searchQuery;

            var recipes = from r in db.Recipes select r;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                recipes = recipes.Where(r => r.Name.Contains(searchQuery));
            }

            if (string.IsNullOrEmpty(sort))
            {
                sort = "Name";
            }

            switch (sort.ToLower())
            {
                case "name desc":
                    recipes = recipes.OrderByDescending(r => r.Name);
                    break;
                case "viewed":
                    recipes = recipes.OrderBy(r => r.LastViewed);
                    break;
                case "viewed desc":
                    recipes = recipes.OrderByDescending(r => r.LastViewed);
                    break;
                case "name":
                default:
                    recipes = recipes.OrderBy(r => r.Name);
                    break;
            }
            return View(recipes);
        }

        //
        // GET: /Recipe/Details/5

        public ActionResult Details(int id = 0)
        {
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            recipe.LastViewed = DateTime.Now;
            db.Entry(recipe).State = EntityState.Modified;
            db.SaveChanges();

            return View(recipe);
        }

        //
        // GET: /Recipe/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Recipe/Create

        [HttpPost]
        public ActionResult Create(Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                recipe.DateAdded = DateTime.Now;
                recipe.LastViewed = DateTime.Now;
                db.Recipes.Add(recipe);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(recipe);
        }

        //
        // GET: /Recipe/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        //
        // POST: /Recipe/Edit/5

        [HttpPost]
        public ActionResult Edit(Recipe recipe)
        {
            if (ModelState.IsValid)
            {
                db.Entry(recipe).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = recipe.ID });
            }
            return View(recipe);
        }

        public ActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Import(string importxml)
        {
            if (string.IsNullOrEmpty(importxml) || !ImportXml(new StringReader(importxml)))
            {
                return RedirectToAction("Create");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ImportFile(HttpPostedFileBase importfile)
        {
            if (importfile != null && importfile.ContentLength > 0)
            {
                if (!ImportXml(new StreamReader(importfile.InputStream)))
                {
                    return RedirectToAction("Create");
                }
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Create");
            }
        }

        private bool ImportXml(TextReader input)
        {
            List<Recipe> recipes = new List<Recipe>();

            try
            {
                using (XmlReader xml = XmlReader.Create(input))
                {
                    xml.ReadToFollowing("recipes");

                    using (XmlReader listReader = xml.ReadSubtree())
                    {

                        while (listReader.ReadToFollowing("recipe"))
                        {
                            Recipe recipe = new Recipe();
                            using (XmlReader recipeReader = listReader.ReadSubtree())
                            {
                                while (!recipeReader.EOF)
                                {
                                    switch (recipeReader.Name)
                                    {
                                        case "name":
                                            recipe.Name = recipeReader.ReadElementContentAsString().Replace("\n", System.Environment.NewLine);
                                            break;

                                        case "ingredients":
                                            recipe.Ingredients = recipeReader.ReadElementContentAsString().Replace("\n", System.Environment.NewLine);
                                            break;

                                        case "method":
                                            recipe.Method = recipeReader.ReadElementContentAsString().Replace("\n", System.Environment.NewLine);
                                            break;

                                        case "notes":
                                            recipe.Notes = recipeReader.ReadElementContentAsString().Replace("\n", System.Environment.NewLine);
                                            break;

                                        case "preptime":
                                            recipe.EstimatedPrepTime = recipeReader.ReadElementContentAsString();
                                            break;
                                        default:
                                            recipeReader.Read();
                                            break;
                                    }
                                }
                            }
                            recipes.Add(recipe);
                        }
                    }
                }

                foreach (Recipe recipe in recipes)
                {
                    recipe.DateAdded = DateTime.Now;
                    recipe.LastViewed = DateTime.Now;
                    db.Recipes.Add(recipe);
                }
                db.SaveChanges();

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public ActionResult Export(int id = 0)
        {
            Recipe recipe = db.Recipes.Find(id);
            StringBuilder exptext = new StringBuilder();
            if (recipe != null)
            {
                using (XmlWriter writer = XmlWriter.Create(exptext))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("recipes");
                    writer.WriteStartElement("recipe");

                    writer.WriteElementString("name",recipe.Name);
                    writer.WriteElementString("ingredients", recipe.Ingredients);
                    writer.WriteElementString("method", recipe.Method);
                    writer.WriteElementString("notes", recipe.Notes);
                    writer.WriteElementString("preptime", recipe.EstimatedPrepTime);
                    
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                Encoding encoder = new UTF8Encoding();
                FileContentResult file = new FileContentResult(encoder.GetBytes(exptext.ToString()), "text/raw");
                file.FileDownloadName = recipe.Name + ".txt";
                return file;
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult ExportAll()
        {
            List<Recipe> recipes = db.Recipes.ToList();
            StringBuilder exptext = new StringBuilder();
            if (recipes != null)
            {
                using (XmlWriter writer = XmlWriter.Create(exptext))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("recipes");

                    foreach (Recipe recipe in recipes)
                    {
                        writer.WriteStartElement("recipe");

                        writer.WriteElementString("name", recipe.Name);
                        writer.WriteElementString("ingredients", recipe.Ingredients);
                        writer.WriteElementString("method", recipe.Method);
                        writer.WriteElementString("notes", recipe.Notes);
                        writer.WriteElementString("preptime", recipe.EstimatedPrepTime);

                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

                Encoding encoder = new UTF8Encoding();
                FileContentResult file = new FileContentResult(encoder.GetBytes(exptext.ToString()), "text/raw");
                file.FileDownloadName = "allrecipes_" + DateTime.Now.ToString("dd-MMMM-yyyy") + ".txt";
                return file;
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //
        // GET: /Recipe/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        //
        // POST: /Recipe/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Recipe recipe = db.Recipes.Find(id);
            db.Recipes.Remove(recipe);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}