using System.IO;
using System.Web;

namespace Texxtoor.BaseLibrary.Core.Optimization {
    public class ImageOptimizationModule : IHttpModule {
        private static readonly object _lockObj = new object();
        private static bool _hasAlreadyRun;

        public void Init(HttpApplication context) {
            lock (_lockObj) {
                if (_hasAlreadyRun) {
                    return;
                }
                else {
                    _hasAlreadyRun = true;
                }
            }

          var spriteDirectoryPhysicalPath = context.Context.Server.MapPath(ImageOptimizations.SpriteDirectoryRelativePath);

          if (Directory.Exists(spriteDirectoryPhysicalPath)) {
            ImageOptimizations.SaveBlankFile(spriteDirectoryPhysicalPath);
            ImageOptimizations.AddCacheDependencies(spriteDirectoryPhysicalPath, rebuildImages: true);
            ImageOptimizations.Initialized = true;
          }
        }

        public void Dispose() {
        }
    }
}