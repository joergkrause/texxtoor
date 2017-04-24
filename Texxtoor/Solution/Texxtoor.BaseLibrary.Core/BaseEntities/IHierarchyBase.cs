using System;
namespace Texxtoor.BaseLibrary.Core.BaseEntities {
  public interface IHierarchyBase<T> {
    System.Collections.Generic.List<T> Children { get; set; }
    bool HasChildren();
    string Name { get; set; }
    int OrderNr { get; set; }
    T Parent { get; }
    string ToString();
  }
}
