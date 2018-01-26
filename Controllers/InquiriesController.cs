using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Student7.Data;
using Student7.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Xrm.Sdk;
using Microsoft.AspNetCore.Authorization;

namespace Student7.Controllers
{
    [Authorize]
    public class InquiriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InquiriesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Inquiries
        public async Task<IActionResult> Index()
        {
            return View(await _context.Inquiry.Where(u=> u.ContactId == Guid.Parse(_userManager.GetUserId(User))).ToListAsync());
        }

        // GET: Inquiries/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquiry = await _context.Inquiry
                .SingleOrDefaultAsync(m => m.InquiryId == id);

            var service = CRM.CrmService.GetServiceProvider();
            var crmInquiry = service.Retrieve("stu7_inquiry", inquiry.InquiryId, new Microsoft.Xrm.Sdk.Query.ColumnSet("stu7_response"));

            inquiry.Response = crmInquiry.GetAttributeValue<string>("stu7_response");
                
            if (inquiry == null)
            {
                return NotFound();
            }

            return View(inquiry);
        }

        // GET: Inquiries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Inquiries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InquiryId,Question,Response,ContactId,UserId")] Inquiry inquiry)
        {
            if (ModelState.IsValid)
            {
                inquiry.InquiryId = Guid.NewGuid();
                inquiry.ContactId = Guid.Parse(_userManager.GetUserId(User));
                _context.Add(inquiry);
                await _context.SaveChangesAsync();

                Entity crmInquiry = new Entity("stu7_inquiry");
                crmInquiry.Id = inquiry.InquiryId;
                crmInquiry["stu7_name"] = _userManager.GetUserName(User);
                crmInquiry["stu7_question"] = inquiry.Question;
                crmInquiry["stu7_contact"] = new EntityReference("contact", inquiry.ContactId);
                var service = CRM.CrmService.GetServiceProvider();
                service.Create(crmInquiry);

                return RedirectToAction("Index");
            }
            return View(inquiry);
        }

        // GET: Inquiries/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            

            var inquiry = await _context.Inquiry.SingleOrDefaultAsync(m => m.InquiryId == id);
            var service = CRM.CrmService.GetServiceProvider();
            var crmInquiry = service.Retrieve("stu7_inquiry", inquiry.InquiryId, new Microsoft.Xrm.Sdk.Query.ColumnSet("stu7_response"));

            inquiry.Response = crmInquiry.GetAttributeValue<string>("stu7_response");
            if (inquiry == null)
            {
                return NotFound();
            }
            return View(inquiry);
        }

        // POST: Inquiries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("InquiryId,Question,Response,ContactId,UserId")] Inquiry inquiry)
        {
            if (id != inquiry.InquiryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inquiry);

                    var service = CRM.CrmService.GetServiceProvider();
                    var crmInquiry = service.Retrieve("stu7_inquiry", inquiry.InquiryId, new Microsoft.Xrm.Sdk.Query.ColumnSet("stu7_response"));
                    crmInquiry["stu7_question"] = inquiry.Question;
                    service.Update(crmInquiry);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InquiryExists(inquiry.InquiryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(inquiry);
        }

        // GET: Inquiries/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inquiry = await _context.Inquiry
                .SingleOrDefaultAsync(m => m.InquiryId == id);
            if (inquiry == null)
            {
                return NotFound();
            }

            return View(inquiry);
        }

        // POST: Inquiries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var inquiry = await _context.Inquiry.SingleOrDefaultAsync(m => m.InquiryId == id);
            _context.Inquiry.Remove(inquiry);
            await _context.SaveChangesAsync();

            var service = CRM.CrmService.GetServiceProvider();
            service.Delete("stu7_inquiry", id);

            return RedirectToAction("Index");
        }

        private bool InquiryExists(Guid id)
        {
            return _context.Inquiry.Any(e => e.InquiryId == id);
        }
    }
}
