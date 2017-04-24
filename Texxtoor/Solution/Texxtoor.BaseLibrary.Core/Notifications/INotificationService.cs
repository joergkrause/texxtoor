using System;
namespace Texxtoor.BaseLibrary.Core.Notifications {
  public interface INotificationService {
    void ApplyTileValue(string target, int value);
    void Notify(string message);
    void Reset();
    void SetProductionProgress(int percent, string message);
    void SetUserOnline();
  }
}
