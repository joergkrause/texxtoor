function ImageCrop(author) {
  this.aspectRatio = 0;
  this.jcrop_api = null;
  this.cropCoords = null;
  this.bounds = null;
  this.imageSize = [];
  this.properties = null;
  this.snippetId = null;
  this.imageContainer = null;
  this.currentImage = null;
  this.scaleX = 0;
  this.scaleY = 0;
  this.author = author;
};

ImageCrop.prototype = {
  aspectRatio: 0,
  jcrop_api: null,
  cropCoords: null,
  bounds: null,
  imageSize: [],
  properties: null,
  snippetId: null,
  imageContainer: null,
  currentImage: null,
  scaleX: 0,
  scaleY: 0,
  author: {},
  release: function () {
    $('.ratio-input, .cropvalues').spinner({
      min: 0,
      max: 9999,
      step: 1,
      increment: 'fast'
    });
    $('.ratio-input:eq(0), [name=crop-width]').val(this.imageSize[0]);
    $('.ratio-input:eq(1), [name=crop-height]').val(this.imageSize[1]);
    $(".ratio-input").attr("disabled", "disabled");
  },
  readProperties: function () {
    if (this.snippetId == null) return;
    this.properties = $.parseJSON($("#sn_block-" + this.snippetId + " input[name=properties]").val());
    this.imageSize = [$("#sn_block-" + this.snippetId + " input[name=width]").val(), $("#sn_block-" + this.snippetId + " input[name=height]").val()];
  },
  setSnippetId: function (id) {
    this.snippetId = id;
  },
  loadImageContainer: function () {
    this.imageContainer = $("#imageContainer");
  },
  setImage: function (url, callback) {
    this.readProperties();
    this.imageContainer.html('');
    this.currentImage = $("<img />");
    this.currentImage.attr('src', url).attr('alt', '');
    this.imageContainer.append(this.currentImage);
    var w = $("#sn_block-" + this.snippetId + " input[name=originalwidth]").val();
    var h = $("#sn_block-" + this.snippetId + " input[name=originalheight]").val();
    this.imageSize = [w, h];
    this.aspectRatio = w / h;
    this.currentImage.css({
      maxWidth: this.imageContainer.width(),
      maxHeight: this.imageContainer.height()
    });
    var $this = this;
    this.currentImage.load(function () {      
      $this.initCrop();
      if ($.isFunction(callback)) {
        callback();
      }
    });
  },
  setOptions: function (opts) {
    this.jcrop_api.setOptions(opts);
  },
  setRatio: function (ratio) {
    this.aspectRatio = ratio;
    this.setOptions({ aspectRatio: ratio });
  },
  setSelect: function (selection) {
    if (this.cropCoords == null) return;
    this.cropCoords.x = selection[0];
    this.cropCoords.y = selection[1];
    this.cropCoords.w = selection[2];
    this.cropCoords.h = selection[3];
    $("[name=crop-x]").val(this.cropCoords.x);
    $("[name=crop-y]").val(this.cropCoords.y);
    $("[name=crop-width]").val(this.cropCoords.w);
    $("[name=crop-height]").val(this.cropCoords.h);
    this.setOptions({ setSelect: selection });
  },
  setCropCoords: function () {
    var $this = this;
    if ($this.properties["Crop"] == null) {
      $this.properties["Crop"] = {};
    }
    $this.properties["Crop"].XOffset = Math.round($this.scaleX * $this.cropCoords.x);
    $this.properties["Crop"].YOffset = Math.round($this.scaleY * $this.cropCoords.y);
    $this.properties["Crop"].CropWidth = parseInt($("[name=crop-width]").val());
    $this.properties["Crop"].CropHeight = parseInt($("[name=crop-height]").val());
  },
  saveCropProperties: function () {
    var $this = this;
    this.properties.ImageWidth = this.properties["Crop"].CropWidth;
    this.properties.ImageHeight = this.properties["Crop"].CropHeight;
    $("#sn_block-" + this.snippetId + " input[name=properties]").val(JSON.stringify(this.properties));
    var img = $('#sn_block-' + $this.snippetId + ' div.img');
    $(img).width(this.properties.ImageWidth);
    $(img).height(this.properties.ImageHeight);
  },
  initCrop: function () {
    var $this = this;
    $this.currentImage.Jcrop({
      bgFade: true,
      bgOpacity: .8,
      onChange: function (coords) {
        $this.cropCoords = coords;
        $this.setCropCoords();
        $("[name=crop-width]").val(Math.round($this.scaleX * coords.w));
        $("[name=crop-height]").val(Math.round($this.scaleY * coords.h));
        $("[name=crop-x]").val(Math.round($this.scaleX * coords.x));
        $("[name=crop-y]").val(Math.round($this.scaleY * coords.y));
      }
    }, function () {
      $this.jcrop_api = this;
      $this.bounds = this.getBounds();
      $this.scaleX = $this.imageSize[0] / $this.bounds[0];
      $this.scaleY = $this.imageSize[1] / $this.bounds[1];
    });
    if (this.properties["Crop"] != null && (2 < $this.properties["Crop"].XOffset + $this.properties["Crop"].YOffset + $this.properties["Crop"].CropWidth + $this.properties["Crop"].CropHeight)) {
      var x1 = $this.properties["Crop"].XOffset / this.scaleX;
      var y1 = $this.properties["Crop"].YOffset / this.scaleY;
      var x2 = ($this.properties["Crop"].CropWidth / this.scaleX + x1);
      var y2 = ($this.properties["Crop"].CropHeight / this.scaleY + y1);
      $this.setOptions({ setSelect: [x1, y1, x2, y2] });
    }
  }
};