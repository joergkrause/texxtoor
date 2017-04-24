svgEditor.setConfig({
  dimensions: [800, 600],
  canvas_expansion: 3,
  initFill: {
    color: '0000ff'
  },
  initStroke: {
    width: 2
  },
  noStorageOnLoad: true,
  forceStorage: true,
  extensions: [
    'ext-server_opensave_texxtoor.js'
  ],
  projectId: projectId,
  resourceId: resourceId,
  imgPath: '/Scripts/svgedit/images/',
  serverServiceloadSvg: serviceUrl.loadSvg,
  serverServicesaveSvg: serviceUrl.saveSvg,
  serverServicesaveImage: serviceUrl.saveImage,
  serverServiceProjectLibrary: serviceUrl.projectLibrary + '?id=' + projectId
});