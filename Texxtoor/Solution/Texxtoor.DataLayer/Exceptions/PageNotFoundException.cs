using System;

namespace Texxtoor.DataModels.Exceptions {
  public class PageNotFoundException : CmsException {
    public PageNotFoundException() {
    }

    public PageNotFoundException(string message, Exception innerException)
      : base(message,
              innerException) {
    }

    public PageNotFoundException(string message)
      : base(message) {
    }
  }
}