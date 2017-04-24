using System;
using System.Collections.Generic;
using System.Linq;
using Texxtoor.DataModels.Models.JobPortal;

namespace Texxtoor.BusinessLayer {

  public class JobPortalManager : Manager<JobPortalManager> {

    public List<JobApplication> GetApplications(string userName) {
      var apps = Ctx.JobAdvertisments.Where(j => j.Applications.Any(a => a.Applicant.UserName == userName))
        .SelectMany(j => j.Applications)
        .Where(a => a.Applicant.UserName == userName)
        .ToList();
      return apps;
    }

    public JobAdvertisment GetJobAdvertisment(int id) {
      return Ctx.JobAdvertisments.FirstOrDefault(j => j.Id == id);
    }

    public JobAdvertisment GetJobAdvertisment(int id, string userName) {
      return Ctx.JobAdvertisments.FirstOrDefault(j => j.Id == id && j.Contact.UserName == userName);
    }

    public void Apply(int id, JobApplication ja) {
      var jad = Ctx.JobAdvertisments.FirstOrDefault(j => j.Id == id);
      jad.Applications.Add(ja);
      SaveChanges();
    }


    public void EditProfile(JobCV cv) {
      Ctx.ApplyValues<JobCV>(cv);
      SaveChanges();
    }


    public JobCV GetDefaultProfile(int id, string userName) {
      var cv = Ctx.JobCVs.FirstOrDefault(c => c.Id == id && c.UserProfile.User.UserName == userName);
      if (cv == null) {
        cv.UserProfile = Manager<UserProfileManager>.Instance.GetProfileByUser(userName);
        cv.VisibleFrom = DateTime.Now;
        cv.VisibleTo = DateTime.Now.AddDays(60);
      }
      return cv;
    }

    public IEnumerable<JobAdvertisment> GetJobs(string userName) {
      return Ctx.JobAdvertisments.Where(j => j.Contact.UserName == userName);
    }

    public JobApplication GetApplication(int id, string UserName) {
      return Ctx.JobAdvertisments
        .Single(j => j.Applications.Any(a => a.Id == id))
        .Applications
        .First(a => a.Id == id);
    }

    public void AnswerApplication(int id, string answer, bool reject, string userName) {
      var japp = JobPortalManager.Instance.GetApplication(id, userName);
      japp.Answer = answer;
      japp.Rejected = reject;
      Ctx.SaveChanges();
    }

    public IEnumerable<JobAdvertisment> GetJobAdvertismentByContact(int id) {
      return Ctx.JobAdvertisments.Where(j => j.Contact.Id == id);
    }

    public void EditJobAdvertisment(int id, JobAdvertisment jad) {
      Ctx.ApplyValues<JobAdvertisment>(jad);
      SaveChanges();
    }

    public void DeleteJobAdvertisment(int id, string userName) {
      var ja = Ctx.JobAdvertisments.FirstOrDefault(j => j.Contact.UserName == userName && j.Id == id);
      if (ja != null) {
        Ctx.JobAdvertisments.Remove(ja);
        SaveChanges();
      }
    }

    public void CreateJobAdvertisment(JobAdvertisment jobAd, string userName) {
      jobAd.Contact = GetCurrentUser(userName);
      jobAd.CompanyProfile = Manager<UserProfileManager>.Instance.GetProfileByUser(userName);
      Ctx.JobAdvertisments.Add(jobAd);
      SaveChanges();
    }

    public IEnumerable<JobCategory> GetAllCategories() {
      return Ctx.JobCategories.Where(c => c.LocaleId == CurrentCulture).ToList();
    }

    public void ApplyJobAdvertismentValues(JobAdvertisment jad, string ct, string wt, string ca, string userName) {
      var oldJad = Ctx.JobAdvertisments.Find(jad.Id) ?? jad;
      jad.CopyProperties<JobAdvertisment>(oldJad, 
        a => a.JobLongDescription,
        a => a.JobShortDescription,
        a => a.JobTitle,
        a => a.Reference,
        a => a.Regions,
        a => a.VisibleTo,
        a => a.VisibleFrom
        );
      oldJad.ContractTypes = (JobContractType)ct.Split(new[] { ',' }).ToList().Sum(s => Int32.Parse(s));
      oldJad.WorkTypes = (JobStatuteType)wt.Split(new[] { ',' }).ToList().Sum(s => Int32.Parse(s));
      oldJad.Categories = Ctx.JobCategories.ToList().Where(c => ca.Split(new[] { ',' }).Any(i => Int32.Parse(i) == c.Id)).ToList();
      oldJad.CompanyProfile = Ctx.UserProfiles.FirstOrDefault(u => u.User.UserName == userName);
      oldJad.Contact = Ctx.Users.FirstOrDefault(u => u.UserName == userName);
      SaveChanges();
    }

    public IEnumerable<JobAdvertisment> SearchJob(Texxtoor.ViewModels.Jobs.SearchJob sj) {
      var st = sj.SearchTerm;
      var td = DateTime.Now;
      return Ctx.JobAdvertisments
                .Where(j => j.VisibleFrom <= td && j.VisibleTo >= td)
                .Where(
                  j =>
                  j.JobTitle.Contains(st) || j.JobShortDescription.Contains(st) || j.JobLongDescription.Contains(st))
                .ToList()
                .Where(
                  j =>
                  (((int)sj.ContractTypes == 0) || ((int)j.ContractTypes & (int)sj.ContractTypes) == (int)sj.ContractTypes)
                  &&
                  (((int)sj.WorkTypes == 0) || ((int)j.WorkTypes & (int)sj.WorkTypes) == (int)sj.WorkTypes)
                  );
    }
  }
}
