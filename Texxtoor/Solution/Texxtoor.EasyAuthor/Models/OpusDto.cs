namespace Texxtoor.EasyAuthor.Models {
  using System.Collections.Generic;
  using System.Linq;

  using BaseLibrary;
  using DataModels.Models.Content;
  using Utilities;
  using System;

  public class OpusDto {
    public OpusDto() {
    }

    public OpusDto(string userName, Opus opus, ProjectManager projectUnitOfWork, ReaderManager readerManager) {
      Id = opus.Id;
      IsPublished = opus.IsPublished;
      IsFallback = opus.IsFallback;
      OpusName = opus.Name;
      ProjectId = opus.Project.Id;

      // on display: TRUE --> Author; FALSE --> Co-Author
      IsAuthor = opus.UserIsTeamLead(userName);
      TeamMemberRole = opus.Project
        .Team
        .Members.Single(m => m.Member.UserName == userName)
        .Role.ContributorRoles.ToString();
      Authors = opus.Project.Team.Members
        .OrderByDescending(x => x.TeamLead)
        .Select(x => new MemberDto {
          MemberId = x.Id,
          FullName = x.Member.Profile.FullName,
          Roles = x.GetLocalizedContributorRoles(),
          ThumbnailPath = String.Format(Constants.Images.ApiUrlTemplate, x.Member.Profile.Id, Constants.Images.StandardTypes.Profile)
        });

      // JOERG:
      // We need Name; Authors; Role of User; New Messages; Cover Image
      if (opus.IsPublished && opus.Published != null) {
        Title = opus.Published.Title;
        Description = opus.Project.Description;
        CoverUrl = String.Format(Constants.Images.ApiUrlTemplate, opus.Published.Id, Constants.Images.StandardTypes.Project);
        NewMessages = projectUnitOfWork.GetTopWorkroomMessages(opus.Published.Id)
            .Where(x => !x.Closed)
            .Select(x => new CommentDto {
              Subject = x.Name,
              Text = x.Content,
              UserName = x.Owner.UserName
            });

        PublishedNewMessages = readerManager.GetPublishedComments(opus.Published.Id)
            .Where(x => !x.Closed)
            .Select(x => new CommentDto {
              Subject = x.Name,
              Text = x.Content,
              UserName = x.Owner.UserName
            }); ;
        SubTitle = opus.Published.SubTitle;
      } else {
        Title = opus.Project.Name;
        Description = opus.Project.Description;
        CoverUrl = string.Format(Constants.Images.ApiUrlTemplate, opus.Project.Id, Constants.Images.StandardTypes.Project);
        NewMessages = projectUnitOfWork.GetTopWorkroomMessages(opus.Project.Id)
            .Where(x => !x.Closed)
            .Select(x => new CommentDto {
              Subject = x.Name,
              Text = x.Content,
              UserName = x.Owner.UserName
            });
      }
    }

    public int ProjectId { get; set; }

    public string TeamMemberRole { get; set; }

    public IEnumerable<MemberDto> Authors { get; set; }

    public int Id { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFallback { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string OpusName { get; set; }

    public string CoverUrl { get; set; }

    public bool IsAuthor { get; set; }

    public string SubTitle { get; set; }

    public IEnumerable<CommentDto> NewMessages { get; set; }

    public IEnumerable<CommentDto> PublishedNewMessages { get; set; }
  }
}