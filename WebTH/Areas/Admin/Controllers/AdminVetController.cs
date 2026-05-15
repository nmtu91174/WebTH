using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebTH.Models;

namespace WebTH.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminVetController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AdminVet/ClinicList
        public ActionResult ClinicList(int page = 1)
        {
            const int pageSize = 10;
            var clinics = db.VetClinics
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)db.VetClinics.Count() / pageSize);
            ViewBag.TotalClinics = db.VetClinics.Count();
            ViewBag.VerifiedCount = db.VetClinics.Count(c => c.IsVerified);
            ViewBag.PendingCount = db.VetClinics.Count(c => !c.IsVerified);

            return View(clinics);
        }

        // GET: AdminVet/CreateClinic
        public ActionResult CreateClinic()
        {
            return View();
        }

        // POST: AdminVet/CreateClinic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateClinic(VetClinic clinic)
        {
            if (ModelState.IsValid)
            {
                clinic.CreatedDate = DateTime.Now;
                clinic.CreatedBy = User.Identity.Name;
                clinic.IsActive = true;
                clinic.IsVerified = false;
                clinic.TotalRecommendations = 0;
                clinic.AverageRating = 0;

                db.VetClinics.Add(clinic);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Phòng khám đã được tạo thành công!";
                return RedirectToAction("ClinicList");
            }

            return View(clinic);
        }

        // GET: AdminVet/EditClinic/:id
        public ActionResult EditClinic(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            VetClinic clinic = db.VetClinics.Find(id);
            if (clinic == null) return HttpNotFound();

            return View(clinic);
        }

        // POST: AdminVet/EditClinic/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditClinic(VetClinic clinic)
        {
            if (ModelState.IsValid)
            {
                var existingClinic = db.VetClinics.Find(clinic.Id);
                if (existingClinic == null) return HttpNotFound();

                existingClinic.ClinicName = clinic.ClinicName;
                existingClinic.Phone = clinic.Phone;
                existingClinic.Email = clinic.Email;
                existingClinic.Website = clinic.Website;
                existingClinic.Address = clinic.Address;
                existingClinic.City = clinic.City;
                existingClinic.District = clinic.District;
                existingClinic.Description = clinic.Description;
                existingClinic.YearsInBusiness = clinic.YearsInBusiness;
                existingClinic.SpecializationAreas = clinic.SpecializationAreas;
                existingClinic.Logo = clinic.Logo;
                existingClinic.ModifiedDate = DateTime.Now;
                existingClinic.ModifiedBy = User.Identity.Name;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Phòng khám đã được cập nhật thành công!";
                return RedirectToAction("ClinicList");
            }

            return View(clinic);
        }

        // POST: AdminVet/VerifyClinic/:id
        [HttpPost]
        public ActionResult VerifyClinic(int id)
        {
            var clinic = db.VetClinics.Find(id);
            if (clinic == null) return HttpNotFound();

            clinic.IsVerified = true;
            clinic.ModifiedDate = DateTime.Now;
            clinic.ModifiedBy = User.Identity.Name;
            db.SaveChanges();

            return Json(new { success = true, message = "Phòng khám đã được xác minh!" });
        }

        // POST: AdminVet/DeleteClinic/:id
        [HttpPost]
        public ActionResult DeleteClinic(int id)
        {
            var clinic = db.VetClinics.Find(id);
            if (clinic == null) return HttpNotFound();

            clinic.IsActive = false;
            clinic.ModifiedDate = DateTime.Now;
            clinic.ModifiedBy = User.Identity.Name;
            db.SaveChanges();

            return Json(new { success = true, message = "Phòng khám đã bị vô hiệu hóa!" });
        }

        // GET: AdminVet/DoctorList/:id
        public ActionResult DoctorList(int? id, int page = 1)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var clinic = db.VetClinics.Find(id);
            if (clinic == null) return HttpNotFound();

            const int pageSize = 10;
            var doctors = db.VetDoctors
                .Where(d => d.ClinicId == id)
                .OrderByDescending(d => d.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ClinicId = id;
            ViewBag.ClinicName = clinic.ClinicName;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)db.VetDoctors.Count(d => d.ClinicId == id) / pageSize);

            return View(doctors);
        }

        // GET: AdminVet/CreateDoctor/:clinicId
        public ActionResult CreateDoctor(int? clinicId)
        {
            if (clinicId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var clinic = db.VetClinics.Find(clinicId);
            if (clinic == null) return HttpNotFound();

            ViewBag.ClinicId = clinicId;
            ViewBag.ClinicName = clinic.ClinicName;
            return View();
        }

        // POST: AdminVet/CreateDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDoctor(VetDoctor doctor)
        {
            if (ModelState.IsValid)
            {
                var clinic = db.VetClinics.Find(doctor.ClinicId);
                if (clinic == null) return HttpNotFound();

                doctor.CreatedDate = DateTime.Now;
                doctor.CreatedBy = User.Identity.Name;
                doctor.IsActive = true;

                db.VetDoctors.Add(doctor);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Bác sĩ đã được thêm thành công!";
                return RedirectToAction("DoctorList", new { id = doctor.ClinicId });
            }

            ViewBag.ClinicId = doctor.ClinicId;
            ViewBag.ClinicName = db.VetClinics.Find(doctor.ClinicId).ClinicName;
            return View(doctor);
        }

        // GET: AdminVet/EditDoctor/:id
        public ActionResult EditDoctor(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var doctor = db.VetDoctors.Find(id);
            if (doctor == null) return HttpNotFound();

            ViewBag.ClinicName = doctor.VetClinic.ClinicName;
            return View(doctor);
        }

        // POST: AdminVet/EditDoctor/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDoctor(VetDoctor doctor)
        {
            if (ModelState.IsValid)
            {
                var existingDoctor = db.VetDoctors.Find(doctor.Id);
                if (existingDoctor == null) return HttpNotFound();

                existingDoctor.DoctorName = doctor.DoctorName;
                existingDoctor.Qualifications = doctor.Qualifications;
                existingDoctor.Specialization = doctor.Specialization;
                existingDoctor.Avatar = doctor.Avatar;
                existingDoctor.Bio = doctor.Bio;
                existingDoctor.ExperienceYears = doctor.ExperienceYears;
                existingDoctor.ModifiedDate = DateTime.Now;
                existingDoctor.ModifiedBy = User.Identity.Name;

                db.SaveChanges();
                TempData["SuccessMessage"] = "Bác sĩ đã được cập nhật thành công!";
                return RedirectToAction("DoctorList", new { id = existingDoctor.ClinicId });
            }

            return View(doctor);
        }

        // POST: AdminVet/DeleteDoctor/:id
        [HttpPost]
        public ActionResult DeleteDoctor(int id)
        {
            var doctor = db.VetDoctors.Find(id);
            if (doctor == null) return HttpNotFound();

            int clinicId = doctor.ClinicId;
            doctor.IsActive = false;
            doctor.ModifiedDate = DateTime.Now;
            doctor.ModifiedBy = User.Identity.Name;
            db.SaveChanges();

            return Json(new { success = true, message = "Bác sĩ đã bị xóa!", clinicId = clinicId });
        }

        // GET: AdminVet/RecommendationList/:clinicId
        public ActionResult RecommendationList(int? clinicId, string filter = "pending", int page = 1)
        {
            const int pageSize = 10;
            IQueryable<VetRecommendation> query = db.VetRecommendations
                .Include(r => r.Product)
                .Include(r => r.VetDoctor)
                .OrderByDescending(r => r.CreatedDate);

            if (clinicId.HasValue)
            {
                var clinic = db.VetClinics.Find(clinicId.Value);
                if (clinic == null) return HttpNotFound();
                query = query.Where(r => r.ClinicId == clinicId.Value);
                ViewBag.ClinicName = clinic.ClinicName;
            }
            else
            {
                ViewBag.ClinicName = "Tất cả phòng khám";
            }

            if (filter == "approved")
            {
                query = query.Where(r => r.IsApproved);
            }
            else if (filter == "pending")
            {
                query = query.Where(r => !r.IsApproved);
            }
            else if (filter == "rejected")
            {
                query = query.Where(r => r.IsApproved == false && r.ModifiedBy != null);
            }

            var recommendations = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ClinicId = clinicId;
            ViewBag.Filter = filter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize);
            ViewBag.PendingCount = clinicId.HasValue
                ? db.VetRecommendations.Count(r => r.ClinicId == clinicId.Value && !r.IsApproved)
                : db.VetRecommendations.Count(r => !r.IsApproved);
            ViewBag.ApprovedCount = clinicId.HasValue
                ? db.VetRecommendations.Count(r => r.ClinicId == clinicId.Value && r.IsApproved)
                : db.VetRecommendations.Count(r => r.IsApproved);
            ViewBag.ClinicSelectList = new SelectList(
                db.VetClinics
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.ClinicName)
                    .ToList(),
                "Id",
                "ClinicName",
                clinicId);

            return View(recommendations);
        }

        // GET: AdminVet/CreateRecommendation
        public ActionResult CreateRecommendation(int? clinicId = null)
        {
            var model = new VetRecommendation
            {
                ClinicId = clinicId ?? 0,
                RecommendationLevel = 1,
                IsApproved = false
            };

            PopulateRecommendationDropdowns(model.ClinicId > 0 ? (int?)model.ClinicId : null, null, null);
            return View(model);
        }

        // POST: AdminVet/CreateRecommendation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateRecommendation(VetRecommendation model)
        {
            var selectedClinic = db.VetClinics.FirstOrDefault(c => c.Id == model.ClinicId && c.IsActive);
            if (model.ClinicId <= 0 || selectedClinic == null)
            {
                ModelState.AddModelError("ClinicId", "Vui lòng chọn phòng khám hợp lệ.");
            }

            var selectedProduct = db.Products.FirstOrDefault(p => p.Id == model.ProductId && p.IsActive);
            if (model.ProductId <= 0 || selectedProduct == null)
            {
                ModelState.AddModelError("ProductId", "Vui lòng chọn sản phẩm hợp lệ.");
            }

            if (model.DoctorId.HasValue)
            {
                var doctor = db.VetDoctors.FirstOrDefault(d => d.Id == model.DoctorId.Value && d.ClinicId == model.ClinicId);
                if (doctor == null)
                {
                    ModelState.AddModelError("DoctorId", "Bác sĩ không thuộc phòng khám đã chọn.");
                }
            }

            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.ModifiedDate = DateTime.Now;
                model.CreatedBy = User.Identity.Name;
                model.ModifiedBy = User.Identity.Name;

                if (model.IsApproved)
                {
                    model.ApprovedBy = User.Identity.Name;
                    model.ApprovedDate = DateTime.Now;
                }
                else
                {
                    model.ApprovedBy = null;
                    model.ApprovedDate = null;
                }

                db.VetRecommendations.Add(model);

                if (selectedClinic != null)
                {
                    selectedClinic.TotalRecommendations = db.VetRecommendations.Count(r => r.ClinicId == selectedClinic.Id && r.IsApproved)
                                               + (model.IsApproved ? 1 : 0);
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Đã thêm khuyến cáo thành công.";
                return RedirectToAction("RecommendationList", new { clinicId = model.ClinicId, filter = "all" });
            }

            PopulateRecommendationDropdowns(model.ClinicId, model.DoctorId, model.ProductId);
            return View(model);
        }

        // GET: AdminVet/EditRecommendation/:id
        public ActionResult EditRecommendation(int? id, int? clinicId = null, string filter = "all", int page = 1)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var recommendation = db.VetRecommendations.Find(id.Value);
            if (recommendation == null) return HttpNotFound();

            PopulateRecommendationDropdowns(recommendation.ClinicId, recommendation.DoctorId, recommendation.ProductId);
            ViewBag.ReturnClinicId = clinicId;
            ViewBag.ReturnFilter = string.IsNullOrWhiteSpace(filter) ? "all" : filter;
            ViewBag.ReturnPage = page < 1 ? 1 : page;

            return View(recommendation);
        }

        // POST: AdminVet/EditRecommendation/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRecommendation(VetRecommendation model, int? clinicId = null, string filter = "all", int page = 1)
        {
            var recommendation = db.VetRecommendations.Find(model.Id);
            if (recommendation == null) return HttpNotFound();

            var selectedClinic = db.VetClinics.FirstOrDefault(c => c.Id == model.ClinicId && c.IsActive);
            if (model.ClinicId <= 0 || selectedClinic == null)
            {
                ModelState.AddModelError("ClinicId", "Vui lòng chọn phòng khám hợp lệ.");
            }

            var selectedProduct = db.Products.FirstOrDefault(p => p.Id == model.ProductId && p.IsActive);
            if (model.ProductId <= 0 || selectedProduct == null)
            {
                ModelState.AddModelError("ProductId", "Vui lòng chọn sản phẩm hợp lệ.");
            }

            if (model.DoctorId.HasValue)
            {
                var doctor = db.VetDoctors.FirstOrDefault(d => d.Id == model.DoctorId.Value && d.ClinicId == model.ClinicId);
                if (doctor == null)
                {
                    ModelState.AddModelError("DoctorId", "Bác sĩ không thuộc phòng khám đã chọn.");
                }
            }

            if (!ModelState.IsValid)
            {
                PopulateRecommendationDropdowns(model.ClinicId, model.DoctorId, model.ProductId);
                ViewBag.ReturnClinicId = clinicId;
                ViewBag.ReturnFilter = string.IsNullOrWhiteSpace(filter) ? "all" : filter;
                ViewBag.ReturnPage = page < 1 ? 1 : page;
                return View(model);
            }

            var oldClinicId = recommendation.ClinicId;
            var wasApproved = recommendation.IsApproved;

            recommendation.ClinicId = model.ClinicId;
            recommendation.DoctorId = model.DoctorId;
            recommendation.ProductId = model.ProductId;
            recommendation.PetType = model.PetType;
            recommendation.ReccommendationReason = model.ReccommendationReason;
            recommendation.HealthCondition = model.HealthCondition;
            recommendation.PetAgeRange = model.PetAgeRange;
            recommendation.RecommendationLevel = model.RecommendationLevel;
            recommendation.IsApproved = model.IsApproved;
            recommendation.ModifiedDate = DateTime.Now;
            recommendation.ModifiedBy = User.Identity.Name;

            if (!wasApproved && model.IsApproved)
            {
                recommendation.ApprovedBy = User.Identity.Name;
                recommendation.ApprovedDate = DateTime.Now;
            }
            else if (!model.IsApproved)
            {
                recommendation.ApprovedBy = null;
                recommendation.ApprovedDate = null;
            }

            db.SaveChanges();

            var clinicIdsToUpdate = new List<int> { recommendation.ClinicId };
            if (oldClinicId != recommendation.ClinicId)
            {
                clinicIdsToUpdate.Add(oldClinicId);
            }

            foreach (var itemClinicId in clinicIdsToUpdate.Distinct())
            {
                var clinic = db.VetClinics.Find(itemClinicId);
                if (clinic != null)
                {
                    clinic.TotalRecommendations = db.VetRecommendations.Count(r => r.ClinicId == itemClinicId && r.IsApproved);
                }
            }
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đã cập nhật khuyến cáo thành công.";
            return RedirectToAction("RecommendationList", new
            {
                clinicId = clinicId,
                filter = string.IsNullOrWhiteSpace(filter) ? "all" : filter,
                page = page < 1 ? 1 : page
            });
        }

        [HttpGet]
        public ActionResult GetDoctorsByClinic(int clinicId)
        {
            var doctors = db.VetDoctors
                .Where(d => d.ClinicId == clinicId && d.IsActive)
                .OrderBy(d => d.DoctorName)
                .Select(d => new { d.Id, d.DoctorName })
                .ToList();

            return Json(new { success = true, data = doctors }, JsonRequestBehavior.AllowGet);
        }

        // POST: AdminVet/ApproveRecommendation/:id
        [HttpPost]
        public ActionResult ApproveRecommendation(int id)
        {
            var recommendation = db.VetRecommendations.Find(id);
            if (recommendation == null) return Json(new { success = false, message = "Không tìm thấy khuyến cáo." });

            recommendation.IsApproved = true;
            recommendation.ApprovedBy = User.Identity.Name;
            recommendation.ApprovedDate = DateTime.Now;
            recommendation.ModifiedDate = DateTime.Now;
            recommendation.ModifiedBy = User.Identity.Name;

            // Update clinic recommendation count
            var clinic = recommendation.VetClinic;
            clinic.TotalRecommendations = db.VetRecommendations.Count(r => r.ClinicId == clinic.Id && r.IsApproved);

            db.SaveChanges();
            return Json(new { success = true, message = "Khuyến cáo đã được phê duyệt!" });
        }

        // POST: AdminVet/RejectRecommendation/:id
        [HttpPost]
        public ActionResult RejectRecommendation(int id, string reason = "")
        {
            var recommendation = db.VetRecommendations.Find(id);
            if (recommendation == null) return Json(new { success = false, message = "Không tìm thấy khuyến cáo." });

            db.VetRecommendations.Remove(recommendation);
            db.SaveChanges();

            return Json(new { success = true, message = "Khuyến cáo đã bị từ chối!" });
        }

        // GET: AdminVet/RatingList/:clinicId
        public ActionResult RatingList(int? clinicId, string filter = "all", int page = 1)
        {
            const int pageSize = 10;
            IQueryable<VetClinicRating> query = db.VetClinicRatings
                .Include(r => r.VetClinic)
                .OrderByDescending(r => r.CreatedDate);

            if (clinicId.HasValue)
            {
                var clinic = db.VetClinics.Find(clinicId);
                if (clinic == null) return HttpNotFound();

                query = query.Where(r => r.ClinicId == clinicId.Value);
                ViewBag.ClinicName = clinic.ClinicName;
            }
            else
            {
                ViewBag.ClinicName = "Tất cả phòng khám";
            }

            if (filter == "approved")
            {
                query = query.Where(r => r.IsApproved);
            }
            else if (filter == "pending")
            {
                query = query.Where(r => !r.IsApproved);
            }

            var ratings = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.ClinicId = clinicId;
            ViewBag.Filter = filter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)query.Count() / pageSize);
            ViewBag.PendingCount = clinicId.HasValue
                ? db.VetClinicRatings.Count(r => r.ClinicId == clinicId.Value && !r.IsApproved)
                : db.VetClinicRatings.Count(r => !r.IsApproved);
            ViewBag.ApprovedCount = clinicId.HasValue
                ? db.VetClinicRatings.Count(r => r.ClinicId == clinicId.Value && r.IsApproved)
                : db.VetClinicRatings.Count(r => r.IsApproved);

            return View(ratings);
        }

        // GET: AdminVet/EditRating/:id
        public ActionResult EditRating(int? id, int? clinicId = null, string filter = "all", int page = 1)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var rating = db.VetClinicRatings
                .Include(r => r.VetClinic)
                .FirstOrDefault(r => r.Id == id.Value);

            if (rating == null) return HttpNotFound();

            ViewBag.ClinicName = rating.VetClinic != null ? rating.VetClinic.ClinicName : "Khong xac dinh";
            ViewBag.ReturnClinicId = clinicId;
            ViewBag.ReturnFilter = string.IsNullOrWhiteSpace(filter) ? "all" : filter;
            ViewBag.ReturnPage = page < 1 ? 1 : page;

            return View(rating);
        }

        // POST: AdminVet/EditRating/:id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRating(VetClinicRating model, int? clinicId = null, string filter = "all", int page = 1)
        {
            var rating = db.VetClinicRatings
                .Include(r => r.VetClinic)
                .FirstOrDefault(r => r.Id == model.Id);

            if (rating == null) return HttpNotFound();

            if (!ModelState.IsValid)
            {
                model.VetClinic = rating.VetClinic;
                ViewBag.ClinicName = rating.VetClinic != null ? rating.VetClinic.ClinicName : "Khong xac dinh";
                ViewBag.ReturnClinicId = clinicId;
                ViewBag.ReturnFilter = string.IsNullOrWhiteSpace(filter) ? "all" : filter;
                ViewBag.ReturnPage = page < 1 ? 1 : page;
                return View(model);
            }

            rating.UserName = model.UserName;
            rating.Email = model.Email;
            rating.Phone = model.Phone;
            rating.Rating = model.Rating;
            rating.Comment = model.Comment;
            rating.VisitDate = model.VisitDate;
            rating.ServiceQuality = model.ServiceQuality;
            rating.Professionalism = model.Professionalism;
            rating.Facility = model.Facility;
            rating.PriceValue = model.PriceValue;
            rating.IsVerified = model.IsVerified;
            rating.ModifiedDate = DateTime.Now;
            rating.ModifiedBy = User.Identity.Name;

            if (rating.IsApproved)
            {
                var approvedRatings = db.VetClinicRatings
                    .Where(r => r.ClinicId == rating.ClinicId && r.IsApproved && r.Id != rating.Id)
                    .Select(r => r.Rating)
                    .ToList();

                approvedRatings.Add(rating.Rating);

                if (rating.VetClinic != null)
                {
                    rating.VetClinic.AverageRating = approvedRatings.Any()
                        ? Math.Round((decimal)approvedRatings.Average(), 1)
                        : 0;
                }
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Da cap nhat danh gia thanh cong!";

            return RedirectToAction("RatingList", new
            {
                clinicId = clinicId,
                filter = string.IsNullOrWhiteSpace(filter) ? "all" : filter,
                page = page < 1 ? 1 : page
            });
        }

        // POST: AdminVet/ApproveRating/:id
        [HttpPost]
        public ActionResult ApproveRating(int id)
        {
            var rating = db.VetClinicRatings.Find(id);
            if (rating == null) return Json(new { success = false, message = "Không tìm thấy đánh giá." });

            rating.IsApproved = true;
            rating.ModifiedDate = DateTime.Now;
            rating.ModifiedBy = User.Identity.Name;

            // Recalculate average rating
            var clinic = rating.VetClinic;
            var approvedRatings = db.VetClinicRatings
                .Where(r => r.ClinicId == clinic.Id && r.IsApproved)
                .ToList();

            if (approvedRatings.Any())
            {
                clinic.AverageRating = Math.Round((decimal)approvedRatings.Average(r => r.Rating), 1);
            }

            db.SaveChanges();
            return Json(new { success = true, message = "Đánh giá đã được phê duyệt!" });
        }

        // POST: AdminVet/RejectRating/:id
        [HttpPost]
        public ActionResult RejectRating(int id)
        {
            var rating = db.VetClinicRatings.Find(id);
            if (rating == null) return Json(new { success = false, message = "Không tìm thấy đánh giá." });

            db.VetClinicRatings.Remove(rating);
            db.SaveChanges();

            return Json(new { success = true, message = "Đánh giá đã bị từ chối!" });
        }

        // GET: AdminVet/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.TotalClinics = db.VetClinics.Count(c => c.IsActive);
            ViewBag.VerifiedClinics = db.VetClinics.Count(c => c.IsVerified && c.IsActive);
            ViewBag.TotalDoctors = db.VetDoctors.Count(d => d.IsActive);
            ViewBag.TotalRecommendations = db.VetRecommendations.Count(r => r.IsApproved);
            ViewBag.PendingRecommendations = db.VetRecommendations.Count(r => !r.IsApproved);
            ViewBag.PendingRatings = db.VetClinicRatings.Count(r => !r.IsApproved);
            ViewBag.ApprovedRatings = db.VetClinicRatings.Count(r => r.IsApproved);

            // Top clinics by rating
            var topClinics = db.VetClinics
                .Where(c => c.IsActive)
                .OrderByDescending(c => c.AverageRating)
                .Take(5)
                .ToList();

            ViewBag.TopClinics = topClinics;

            // Recent recommendations
            var recentRecommendations = db.VetRecommendations
                .OrderByDescending(r => r.CreatedDate)
                .Take(10)
                .Include(r => r.VetClinic)
                .Include(r => r.Product)
                .ToList();

            ViewBag.RecentRecommendations = recentRecommendations;

            return View();
        }

        // POST: AdminVet/UploadClinicLogo
        [HttpPost]
        public JsonResult UploadClinicLogo()
        {
            try
            {
                if (Request.Files.Count == 0)
                {
                    return Json(new { success = false, message = "Không có tệp được gửi" });
                }

                var file = Request.Files[0];
                if (file.ContentLength == 0)
                {
                    return Json(new { success = false, message = "Tệp trống" });
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/jpg" };
                if (!allowedTypes.Contains(file.ContentType))
                {
                    return Json(new { success = false, message = "Chỉ hỗ trợ định dạng ảnh: PNG, JPG, GIF, WebP" });
                }

                // Validate file size (5MB max)
                if (file.ContentLength > 5 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Kích thước ảnh không được vượt quá 5MB" });
                }

                // Create upload directory if it doesn't exist
                string uploadDir = Server.MapPath("~/Uploads/Clinic/");
                if (!System.IO.Directory.Exists(uploadDir))
                {
                    System.IO.Directory.CreateDirectory(uploadDir);
                }

                // Generate unique filename
                string fileName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(file.FileName);
                string filePath = System.IO.Path.Combine(uploadDir, fileName);

                // Save file
                file.SaveAs(filePath);

                // Generate URL
                string logoUrl = "/Uploads/Clinic/" + fileName;

                return Json(new { success = true, logoUrl = logoUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private void PopulateRecommendationDropdowns(int? clinicId, int? doctorId, int? productId)
        {
            var clinics = db.VetClinics
                .Where(c => c.IsActive)
                .OrderBy(c => c.ClinicName)
                .ToList();
            ViewBag.ClinicList = new SelectList(clinics, "Id", "ClinicName", clinicId);

            var doctorsQuery = db.VetDoctors.Where(d => d.IsActive);
            if (clinicId.HasValue && clinicId.Value > 0)
            {
                doctorsQuery = doctorsQuery.Where(d => d.ClinicId == clinicId.Value);
            }

            var doctors = doctorsQuery
                .Include(d => d.VetClinic)
                .OrderBy(d => d.DoctorName)
                .ToList()
                .Select(d => new
                {
                    d.Id,
                    DisplayName = d.DoctorName + " (" + (d.VetClinic != null ? d.VetClinic.ClinicName : "N/A") + ")"
                })
                .ToList();
            ViewBag.DoctorList = new SelectList(doctors, "Id", "DisplayName", doctorId);

            var products = db.Products
                .Where(p => p.IsActive)
                .OrderBy(p => p.Title)
                .Select(p => new { p.Id, p.Title })
                .ToList();
            ViewBag.ProductList = new SelectList(products, "Id", "Title", productId);
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
}
