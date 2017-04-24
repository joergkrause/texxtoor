using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Texxtoor.BaseLibrary.Core.BaseEntities;
using Texxtoor.BaseLibrary.Core.Utilities.Pagination;
using Texxtoor.ViewModels.Common;

namespace Texxtoor.ViewModels.Shared.Common {
  public interface IMultilistViewModel {
    bool IsDraggable { get; set; }
    int ItemId { get; set; }
    string ItemTitle { get; set; }
    string ItemModel { get; set; }
    IList<InfoBoxTileViewModel> InfoBoxTiles { get; set; }
    IHtmlString Title { get; set; }
    uint NewBadgePeriod { get; set; }
    IHtmlString SubTitle { get; set; }
    IHtmlString VerboseInfoText { get; set; }
    int OptionFieldItemId { get; set; }
    IList<OptionField> OptionFields { get; set; }
  }

  /// <summary>
  /// Manage the common list view
  /// </summary>
  public class MultilistViewModel<T> : IMultilistViewModel where T : EntityBase {

    public MultilistViewModel() {
      NewBadgePeriod = 2;
      InfoBoxTiles = new List<InfoBoxTileViewModel>();
    }

    public static IList<IMultilistViewModel> Create(IPagedList<T> model,
      bool isDraggable,
      Func<T, string> titlePropExpression,
      Func<T, IHtmlString> titleStyledExpression,
      Func<T, IHtmlString> subtitleStyledExpression,
      string modelPath,
      Func<T, IList<OptionField>> options,
      Func<T, int> optionsFieldItemId,
      Func<T, IHtmlString> verboseText,
      Func<T, IList<InfoBoxTileViewModel>> infoBoxTiles,
      uint newBadgePeriod = 2
      ) {
      var list = new List<MultilistViewModel<T>>();
      foreach (var item in model) {
        list.Add(new MultilistViewModel<T>() {
          IsDraggable = isDraggable,
          ItemId = item.Id,
          ItemTitle = titlePropExpression(item),
          Title = titleStyledExpression(item),
          SubTitle = subtitleStyledExpression(item),
          ItemModel = modelPath,
          OptionFields = options(item),
          OptionFieldItemId = optionsFieldItemId(item),
          InfoBoxTiles = infoBoxTiles(item),
          VerboseInfoText = verboseText(item),
          NewBadgePeriod = newBadgePeriod
        });
      }
      return list.ToList<IMultilistViewModel>();
    }


    public bool IsDraggable { get; set; }

    public int ItemId { get; set; }

    public string ItemTitle { get; set; }

    public string ItemModel { get; set; }

    //public IPagedList<T> PagedList { get; set; }

    public IList<InfoBoxTileViewModel> InfoBoxTiles { get; set; }

    public IHtmlString Title { get; set; }

    public uint NewBadgePeriod { get; set; }

    public IHtmlString SubTitle { get; set; }

    public IHtmlString VerboseInfoText { get; set; }

    public int OptionFieldItemId { get; set; }

    public IList<OptionField> OptionFields { get; set; }

  }

  public abstract class InfoBoxTileViewModel {

    protected string Template { get; set; }

    public abstract IHtmlString GetContent(int rightPosition, string styleColorClass = "");
    public string ColorStyleClass { get; set; }

    public virtual string Explain { get; set; }

  }

  public class InfoBoxTextTileViewModel : InfoBoxTileViewModel {

    public InfoBoxTextTileViewModel() {
      Template = @"<div style=""height: 108px; width: 108px; position: absolute; right:{2}px; text-align:center; margin: -5px;"" class=""{3}"">
                    <span style=""font-size: 3em; line-height: 1.8em;"">{0}</span><br />
                    <span style=""font-size: 1.1em;"">{1}</span>
                   </div>";
    }

    public override IHtmlString GetContent(int rightPosition, string styleColorClass = "") {
      return new HtmlString(String.Format(Template, Value, Explain, rightPosition, styleColorClass));
    }

    public object Value { get; set; }

  }

  public class InfoBoxImageTileViewModel : InfoBoxTileViewModel {

    public InfoBoxImageTileViewModel() {
      Template = @"<div style=""height: 108px; width: 108px; position: absolute; right: {2}px; margin: -5px;""><img src=""{0}"" alt=""{1}"" class=""pull-left img img-thumbnail"" style=""width: 108px; height: 108px"" /></div>";
    }

    public string Src { get; set; }

    public override IHtmlString GetContent(int rightPosition, string styleColorClass = "") {
      return new HtmlString(String.Format(Template, Src, Explain, rightPosition));
    }

  }

  public class InfoBoxStatusTileViewModel : InfoBoxTileViewModel {

    public InfoBoxStatusTileViewModel() {
      Template = @"<div style=""height: 108px; width: 108px; position: absolute; right: {2}px; text-align:center; margin: -5px;"" class=""{3}"">
                    <div style=""background-color: {0}; margin: 15px auto; width: 40px; height: 40px; border-radius: 20px;""></div>
                    <span style=""font-size: 1.1em;"">{1}</span>
                   </div>";
    }

    public override IHtmlString GetContent(int rightPosition, string styleColorClass = "") {
      return new HtmlString(String.Format(Template, Color, Explain, rightPosition, styleColorClass));
    }

    public string Color { get; set; }


  }


}
