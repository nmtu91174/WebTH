using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebTH.Models;

namespace WebTH.Controllers
{
    public class VetController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Vet/Index - Danh sÃ¡ch cÃ¡c phÃ²ng khÃ¡m thÃº y
        public ActionResult Index(string city = "", int page = 1)
        {
            var clinics = db.VetClinics
                .Where(v => v.IsActive && v.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrEmpty(city))
            {
                clinics = clinics.Where(v => v.City.Contains(city));
            }

            // Pagination
            int pageSize = 6;
            int totalRecords = clinics.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var result = clinics
                .OrderByDescending(v => v.AverageRating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentCity = city;

            return View(result);
        }

        // GET: Vet/Details/{id} - Chi tiáº¿t phÃ²ng khÃ¡m + Ä‘Ã¡nh giÃ¡ + khuyáº¿n cÃ¡o
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var clinic = db.VetClinics.Find(id);
            if (clinic == null || !clinic.IsActive)
            {
                return HttpNotFound();
            }

            // Get related doctors
            var doctors = db.VetDoctors
                .Where(d => d.ClinicId == id && d.IsActive)
                .ToList();

            // Get ratings
            var ratings = db.VetClinicRatings
                .Where(r => r.ClinicId == id && r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            // Get recommendations
            var recommendations = db.VetRecommendations
                .Where(r => r.ClinicId == id && r.IsApproved)
                .Include(r => r.Product)
                .Include(r => r.VetDoctor)
                .OrderByDescending(r => r.TotalReferences)
                .ToList();

            // Get average rating
            if (ratings.Any())
            {
                clinic.AverageRating = (decimal)ratings.Average(r => r.Rating);
            }

            var viewModel = new VetClinicDetailViewModel
            {
                Clinic = clinic,
                Doctors = doctors,
                Ratings = ratings,
                Recommendations = recommendations,
                AverageRating = clinic.AverageRating,
                TotalReviews = ratings.Count
            };

            return View(viewModel);
        }

        // GET: Vet/ProductRecommendations/{productId} - Khuyáº¿n cÃ¡o tá»« cÃ¡c vet cho sáº£n pháº©m cá»¥ thá»ƒ
        public ActionResult ProductRecommendations(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            var recommendations = db.VetRecommendations
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Include(r => r.VetClinic)
                .Include(r => r.VetDoctor)
                .OrderByDescending(r => r.HelpfulCount)
                .ToList();

            ViewBag.Product = product;
            return View(recommendations);
        }

        // POST: Vet/AddRating - ThÃªm Ä‘Ã¡nh giÃ¡ cho phÃ²ng khÃ¡m
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRating(int clinicId)
        {
            var clinic = db.VetClinics.Find(clinicId);
            if (clinic == null)
            {
                return HttpNotFound();
            }

            var userName = (Request["UserName"] ?? string.Empty).Trim();
            var email = (Request["Email"] ?? string.Empty).Trim();
            var phone = (Request["Phone"] ?? string.Empty).Trim();
            var comment = (Request["Comment"] ?? string.Empty).Trim();

            int ratingValue;
            if (!int.TryParse((Request["Rating"] ?? string.Empty).Trim(), out ratingValue))
            {
                TempData["Error"] = "Danh gia khong hop le. Vui long chon tu 1 den 5 sao.";
                return RedirectToAction("Details", new { id = clinicId });
            }

            if (ratingValue < 1 || ratingValue > 5)
            {
                TempData["Error"] = "Danh gia khong hop le. Vui long chon tu 1 den 5 sao.";
                return RedirectToAction("Details", new { id = clinicId });
            }

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Vui long nhap day du Ten va Nhan xet chi tiet.";
                return RedirectToAction("Details", new { id = clinicId });
            }

            if (userName.Length > 150)
            {
                TempData["Error"] = "Ten khong duoc vuot qua 150 ky tu.";
                return RedirectToAction("Details", new { id = clinicId });
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                if (email.Length > 150)
                {
                    TempData["Error"] = "Email khong duoc vuot qua 150 ky tu.";
                    return RedirectToAction("Details", new { id = clinicId });
                }

                try
                {
                    var mailAddress = new System.Net.Mail.MailAddress(email);
                    email = mailAddress.Address;
                }
                catch
                {
                    TempData["Error"] = "Email khong dung dinh dang.";
                    return RedirectToAction("Details", new { id = clinicId });
                }
            }

            if (phone.Length > 20)
            {
                TempData["Error"] = "So dien thoai khong duoc vuot qua 20 ky tu.";
                return RedirectToAction("Details", new { id = clinicId });
            }

            DateTime? visitDate = null;
            var visitDateRaw = (Request["VisitDate"] ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(visitDateRaw))
            {
                DateTime parsedVisitDate;
                if (DateTime.TryParseExact(visitDateRaw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedVisitDate))
                {
                    visitDate = parsedVisitDate;
                }
                else
                {
                    TempData["Error"] = "Ngay ghe tham khong hop le.";
                    return RedirectToAction("Details", new { id = clinicId });
                }
            }

            try
            {
                var rating = new VetClinicRating
                {
                    ClinicId = clinicId,
                    UserId = User.Identity.IsAuthenticated ? User.Identity.Name : null,
                    UserName = userName,
                    Email = string.IsNullOrWhiteSpace(email) ? null : email,
                    Phone = string.IsNullOrWhiteSpace(phone) ? null : phone,
                    Rating = ratingValue,
                    Comment = comment,
                    VisitDate = visitDate,
                    IsApproved = true,
                    IsVerified = false,
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                db.VetClinicRatings.Add(rating);
                db.SaveChanges();

                // Update clinic average rating
                var allRatings = db.VetClinicRatings
                    .Where(r => r.ClinicId == clinicId && r.IsApproved)
                    .ToList();

                clinic.AverageRating = allRatings.Any()
                    ? (decimal)allRatings.Average(r => r.Rating)
                    : 0m;
                clinic.ModifiedDate = DateTime.Now;
                db.SaveChanges();

                TempData["Success"] = "Cam on ban da danh gia phong kham!";
                return RedirectToAction("Details", new { id = clinicId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Khong the gui danh gia luc nay. Vui long thu lai. Chi tiet: " + ex.GetBaseException().Message;
                return RedirectToAction("Details", new { id = clinicId });
            }
        }
        // POST: Vet/VoteHelpful - BÃ¬nh chá»n khuyáº¿n cÃ¡o cÃ³ há»¯u Ã­ch
        [HttpPost]
        public ActionResult VoteHelpful(int recommendationId, bool isHelpful)
        {
            var recommendation = db.VetRecommendations.Find(recommendationId);
            if (recommendation == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var userId = System.Web.HttpContext.Current.User.Identity.Name;
            var userName = System.Web.HttpContext.Current.User.Identity.Name;

            // Check if user already voted
            var existingVote = db.VetRecommendationVotes
                .FirstOrDefault(v => v.RecommendationId == recommendationId && v.UserId == userId);

            if (existingVote != null)
            {
                // Update existing vote
                existingVote.IsHelpful = isHelpful;
                db.Entry(existingVote).State = EntityState.Modified;
            }
            else
            {
                // Add new vote
                var vote = new VetRecommendationVote
                {
                    RecommendationId = recommendationId,
                    UserId = userId,
                    UserName = userName,
                    IsHelpful = isHelpful,
                    CreatedDate = DateTime.Now
                };
                db.VetRecommendationVotes.Add(vote);
            }

            db.SaveChanges();

            // Update helpful count
            var helpfulVotes = db.VetRecommendationVotes
                .Where(v => v.RecommendationId == recommendationId && v.IsHelpful)
                .Count();

            recommendation.HelpfulCount = helpfulVotes;
            recommendation.TotalReferences++;
            db.SaveChanges();

            return Json(new { success = true, helpfulCount = helpfulVotes });
        }

        // GET: Vet/Search - TÃ¬m kiáº¿m phÃ²ng khÃ¡m theo tÃªn hoáº·c Ä‘á»‹a Ä‘iá»ƒm
        public ActionResult Search(string q)
        {
            var results = db.VetClinics
                .Where(v => v.IsActive && v.IsVerified &&
                       (v.ClinicName.Contains(q) || 
                        v.Address.Contains(q) || 
                        v.City.Contains(q) ||
                        v.District.Contains(q)))
                .OrderByDescending(v => v.AverageRating)
                .ToList();

            ViewBag.SearchQuery = q;
            return View(results);
        }

        // GET: Vet/TopRated - Top phÃ²ng khÃ¡m Ä‘Æ°á»£c Ä‘Ã¡nh giÃ¡ cao nháº¥t
        public ActionResult TopRated()
        {
            var topClinics = db.VetClinics
                .Where(v => v.IsActive && v.IsVerified)
                .OrderByDescending(v => v.AverageRating)
                .Take(10)
                .ToList();

            return View(topClinics);
        }

        // GET: Vet/BySpecialization - Lá»c phÃ²ng khÃ¡m theo chuyÃªn ngÃ nh
        public ActionResult BySpecialization(string specialization)
        {
            var clinics = db.VetClinics
                .Where(v => v.IsActive && v.IsVerified &&
                       v.SpecializationAreas.Contains(specialization))
                .OrderByDescending(v => v.AverageRating)
                .ToList();

            ViewBag.Specialization = specialization;
            return View("Index", clinics);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModel for clinic details
    public class VetClinicDetailViewModel
    {
        public VetClinic Clinic { get; set; }
        public List<VetDoctor> Doctors { get; set; }
        public List<VetClinicRating> Ratings { get; set; }
        public List<VetRecommendation> Recommendations { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }
}

