function ImageCrop() {
    this.aspectRatio = 0,
    this.jcrop_api = null,
    this.cropCoords = null,
    this.bounds = null,
    this.imageSize = null,
    this.properties = null,
    this.snippetId = null,
    this.imageContainer = null,
    this.currentImage = null,
    this.scaleX = 0,
    this.scaleY = 0
};

ImageCrop.prototype = {
    release: function () {
        $('.ratio-input, .crop-width, .crop-height').spinner({
            min: 0,
            max: 9999,
            step: 1,
            increment: 'fast'
        });
        $('.ratio-input:eq(0), .crop-width').val(this.imageSize[0]);
        $('.ratio-input:eq(1), .crop-height').val(this.imageSize[1]);
        $(".ratio-input").attr("disabled", "disabled");
    },
    readProperties: function () {
        if (this.snippetId == null) return;
        this.properties = $.parseJSON($("input[name='properties-" + this.snippetId + "']").val());
        this.imageSize = [$("input[name='width-" + this.snippetId + "']").val(), $("input[name='height-" + this.snippetId + "']").val()];
    },
    setSnippetId: function (id) {
        this.snippetId = id;
    },
    loadImageContainer: function () {
        this.imageContainer = $("#imageContainer");
    },
    setImage: function (url) {
        this.readProperties();
        this.imageContainer.html('');
        this.currentImage = $("<img />");
        this.currentImage.attr('src', url + "?convertImage=false").attr('alt', '');
        this.imageContainer.append(this.currentImage);
        var w = $("input[name='width-" + this.snippetId + "']").val();
        var h = $("input[name='height-" + this.snippetId + "']").val();
        this.imageSize = [w, h];
        this.aspectRatio = w / h;
        this.currentImage.css({
            maxWidth: this.imageContainer.width(),
            maxHeight: this.imageContainer.height()
        });
        var $this = this;
        this.currentImage.load(function () {
            $this.initCrop();
        });

    },
    setOptions: function (opts) {
        this.jcrop_api.setOptions(opts);
    },
    setRatio: function (ratio) {
        console.log(ratio);
        this.aspectRatio = ratio;
        this.setOptions({ aspectRatio: ratio });
    },
    setSelect: function (selection) {
        if (this.cropCoords == null) return;
        var x = this.cropCoords.x;
        var y = this.cropCoords.y;
        this.setOptions({ setSelect: [x, y, selection[0], selection[1]] });
    },
    setCropCoords: function () {
        var $this = this;
        if ($this.properties["Crop"] == null) {
            $this.properties["Crop"] = {};
        }
        $this.properties["Crop"].XOffset = Math.round($this.scaleX * $this.cropCoords.x);
        $this.properties["Crop"].YOffset = Math.round($this.scaleY * $this.cropCoords.y);
        $this.properties["Crop"].CropWidth = parseInt($(".crop-width").val());
        $this.properties["Crop"].CropHeight = parseInt($(".crop-height").val());
    },
    saveCropProperties: function () {
        var $this = this;
        this.properties.ImageWidth = this.properties["Crop"].CropWidth;
        this.properties.ImageHeight = this.properties["Crop"].CropHeight;
        $("input[name='properties-" + this.snippetId + "']").val(JSON.stringify(this.properties));

        author.deferredSave($('.saveableByTexxtoor[data-item=' + this.snippetId + ']'));
        setTimeout(function () {
            var src = $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src');
            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src', src);
            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").load(function () {
                $(".imagePane input[name='width']").val($this.properties.ImageWidth);
                $(".imagePane input[name='height']").val($this.properties.ImageHeight);
                $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").width($this.properties.ImageWidth);
                $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").height($this.properties.ImageHeight);
            });
        }, 2000);
        $('div.imageCropDialog, .popup-layout').hide();
    },
    initCrop: function () {
        var $this = this;
        $this.currentImage.Jcrop({
            bgFade: true,
            bgOpacity: .8,
            onChange: function (coords) {
                $this.cropCoords = coords;
                $this.setCropCoords();
                $(".crop-width").val(Math.round($this.scaleX * coords.w));
                $(".crop-height").val(Math.round($this.scaleY * coords.h));
            }
        }, function () {
            $this.jcrop_api = this;
            $this.bounds = this.getBounds();
            $this.scaleX = $this.imageSize[0] / $this.bounds[0];
            $this.scaleY = $this.imageSize[1] / $this.bounds[1];
        });
        if (this.properties["Crop"] != null) {
            var x1 = $this.properties["Crop"].XOffset / this.scaleX;
            var y1 = $this.properties["Crop"].YOffset / this.scaleY;
            var x2 = ($this.properties["Crop"].CropWidth / this.scaleX + x1);
            var y2 = ($this.properties["Crop"].CropHeight / this.scaleY + y1);
            $this.setOptions({ setSelect: [x1, y1, x2, y2] });
        }
    }
};