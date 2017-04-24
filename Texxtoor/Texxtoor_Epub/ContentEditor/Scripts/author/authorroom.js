/// <reference path="../jquery/jquery-1.7.2.js" />

$(function () {

    var isCtrl = false;
    $().Ribbon({
        theme: 'windows7',
        backstage: false,
        baseUrl: ribbonUrl
    });
    ui.init();
    $("#tocpane li").live('click', function () {
        author.setCurrentChapter($(this).find(".actionLink").attr("href"));
        return false;
    });
    $(".actionLink").live('click', function () { author.setCurrentChapter($(this).attr("href")); return false; });
    author.editorContainer = $("#editor");
    author.statusBar = $('#statusBar');
    author.messageBar = $('#messageBar');
    author.authorUI.createLinkDialogs();
    $('.popContainer').click(function () {
        var current = $(this).find(".popContainerPopup");
        current.toggle('fast');
        var h = current.parent().height();
        current.height(h);
        $(".popContainerPopup").each(function () {
            if ($(this).data('item') != current.data('item')) { $(this).hide(); }
        });
        return false;
    });
    $(window).bind('keydown', function (e) {
        if (e.ctrlKey) { isCtrl = true; }
        if (e.keyCode == 33) {
            author.jscroll.pageUp();
            return false;
        }
        if (e.keyCode == 34) {
            author.jscroll.pageDown();
            return false;
        }
        if (e.keyCode == 116) {
            author.jscroll.showLoader("Page updated");
        }
       
    });
    $(window).bind('keyup', function (e) {
        isCtrl = false; $(".fakeDiv").remove();
    });
    $('.editor a').live('mousemove', function () {
        if (!isCtrl) return;
        $(".fakeDiv").remove();
        var a = $(this);
        $('<div class="fakeDiv" style="position: absolute; background: white; opacity: 0; cursor: pointer; z-index: 100000;">&nbsp</div>')
                    .appendTo('body')
                    .css({
                        width: $(this).outerWidth() + 'px',
                        height: $(this).outerHeight() + 'px',
                        top: $(this).offset().top + 'px',
                        left: $(this).offset().left + 'px'
                    })
                    .mouseleave(function () { $(this).remove(); isCtrl = false; })
                    .mousemove(function () { if (!isCtrl) $(this).remove(); })
                    .click(function () {
                        $(this).remove(); isCtrl = false;
                        author.redirectToSnippet(parseInt(a.data('chapter')), parseInt(a.data('snippet')));
                    });
    });

    // tool handler
    $('.ribbon-button').click(function () {
        $el = $(this);
        if ($el.attr('disabled') === 'disabled') {
            return false;
        }
        var action = $el.data('action');
        var option = $el.data('option');
        switch ($el.data('command')) {
            case "orb":
                switch (action) {
                    case "close":
                        window.location = closeLocation;
                        break;
                    case "html":
                        window.location = htmlLocation;
                        break;
                    case "epub":
                        window.location = epubLocation;
                        break;
                    case "pdf":
                        window.location = pdfLocation;
                        break;
                }
                break;
            case "figure":
                author.figureCommand(action);
                break;
            case "format":
                author.formatCommand($el, action);
                break;
            case "find":
                author.findCommand();
                break;
            case "replace":
                author.replaceCommand();
                break;
            case "insert":
                switch (action) {
                    case "table":
                        var variation = $el.data('option');
                        var data = $('input:text[name=rows]').val().toString() + ',' + $('input:text[name=cols]').val().toString();
                        author.insertCommand('table', variation, data);
                        break;
                    case "term":
                        author.insertTerm(option, $el.data('value'), $el.text());
                        break;
                    case "img": author.insertCommand(action, null, $el.data('item'));
                        break;
                    default:
                        author.insertCommand(action);
                        break;
                }
                break;
            case "showpopup":
                $('div.internalLinkDialog, div.externalLinkDialog').hide();
                switch (action) {
                    case "comments":
                        author.loadComments();
                        break;
                    case "internal":
                    case "external":
                        author.showPopup(action);
                        break;
                }
                break;
            case "showpane":
                switch (action) {
                    case "naviButton": $("#rightbar").toggle(); break;
                    case "flowButton": $("#leftbar").toggle(); break;
                }
                $('.pane').hide();
                $("." + action).toggle();
                if ($("." + action).is(":visible")) {
                    $("." + action).find("a").click();
                    if (action == "imagePane") {
                        $(".cmd-button").attr("disable", "disable");
                        $(".cmd-button").fadeTo("100", "0.4");
                        $(".imagePane input[type='text']").spinner({
                            min: 0,
                            max: 9999,
                            step: 1,
                            increment: 'fast'
                        });
                        $(".imagePane input[type='text']").spinner("disable");
                    }
                }

                break;
            case "shownavipane":
                if ($("." + action).is(":visible")) { $("." + action).css({ display: 'none' }); if (action == 'flowButton') { $('div.editable_highlight_haschanged').css({ display: 'none' }); author.removeHighlightSnippet(); } }
                else { $("." + action).css({ display: 'block' }) }
                break;
            case "save":
                author.saveSnippet();
                break;
            case "delete":
                author.deleteCommand();
                break;
            case "move":
                author.move(action);
                break;
        }
    });

    // Pane management, TODO: move to UI class

    $('.tablePane').on('click', function () {
        $(".tablePane input[type='text']").spinner({
            min: 0,
            max: 100,
            step: 1,
            increment: 'fast'
        });
    });
    $(".equationContainer :not(img)").live('click', function () {
        $('.pane').hide();
        $('.equationPane').show().find('a').click();
    });
    $('.editableSection, .editableText').live('click', function () {
        $(".cmd-button").attr("disable", "disable");
        $(".cmd-button").fadeTo("100", "0.4");
        $('.ins,.del').parent().addClass('disabled').fadeTo("100", "0.4");
        $('.ins,.del').parent().attr('disabled', 'disabled').fadeTo("100", "0.4");
        $(".imagePane input[type='text']").spinner("disable");
    });
    // show pane when editor focused
    $('.editableSection').live('click', function () {
        if ($('.equationPane').is(':visible')) return;
        $('.pane').hide();
        $('ul.menu').find("li:first a").click();

    });
    $('.editableTable').live('click', function () {
        $('.pane').hide();
        $('.tablePane').show().find('a').click();
    });

    $('.editableImage').live('click mousedown', function () {
        var obj = $.parseJSON($(this).closest('.editableImage').find(".imageEditor input:eq(3)").val());
        clearTimeout(author.imgResizeTimeout);
        $(".cmd-button").removeAttr("disable");
        $(".cmd-button").fadeTo("100", "1");
        $(".imagePane input[type='text']").spinner("enable");
        $(".imagePane input[name='width']").val(obj.ImageWidth);
        $(".imagePane input[name='height']").val(obj.ImageHeight);
        $(".thumbnail-image").each(function () {
            var input = $(this);
            var img = $("<img/>").attr('src', input.val()).attr('title', input.attr('data-title')).css({
                width: 32 + 'px',
                height: 32 + 'px',
                margin: 2 + 'px',
                border: 0,
                verticalAlign: 'top'
            });
            input.parent().append(img);
            input.remove();

        });
        if (!$('.imagePane').is(':visible')) {
            $('.pane').hide();
            $('.imagePane').show().find('a').click();
        }
        $(".imagePane input[type='text']").spinner({
            min: 0,
            max: 9999,
            step: 1,
            increment: 'fast'
        });
        if (obj.KeepSize == true) {
            $(".imagePane input[type='text']").spinner('disable');
            $("#keepsize").attr("checked", "checked");
        } else {
            $(".imagePane input[type='text']").spinner('enable');
            $("#keepsize").removeAttr("checked");
        }
        author.imageSize[0] = $(".imagePane input[name='width']").val();
        author.imageSize[1] = $(".imagePane input[name='height']").val();
    });
    $(".editableText").live('click', function () {
        if ($('.equationPane ul').is(":visible")) return;
        if ($('.imagePane ul').is(":visible") || $('.listingPane ul').is(":visible") || $('.tablePane ul').is(":visible")) {
            $('.pane').hide();
            isClosed = true;
            $('body').animate({ paddingTop: 27 + 'px' }, function () {
                author.jscroll.resize();
            });
        } else
            $('.pane').hide();
    });
    $(".imagePane").click(function () {
        $(".thumbnail-image").each(function () {
            var input = $(this);
            var img = $("<img/>").attr('src', input.val()).attr('title', input.attr('data-title')).css({
                width: 32 + 'px',
                height: 32 + 'px',
                margin: 2 + 'px',
                border: 0,
                verticalAlign: 'top'
            });
            input.parent().append(img);
            input.remove();

        });
    });
    $(".imagePane input[name='width']").bind('blur change keyup', function () {
        author.figureCommand('setsize', $(this));
    });
    $(".imagePane input[name='height']").bind('blur change keyup', function () {
        author.figureCommand('setsize', $(this));
    });

    $('.listingEditor').live('click', function () {
        if (!$('.listingPane').is(':visible')) {
            $('.pane').hide();
            $('.listingPane').show().find('a').click();
        }
        if ($("input[name='syntaxhighlight-" + author.snippetId + "']").val().toLowerCase() === "true") {
            $("#syntaxhighlight").attr("checked", "checked");
        } else {
            $("#syntaxhighlight").removeAttr("checked");
        }
        if ($("input[name='linenumbers-" + author.snippetId + "']").val().toLowerCase() === "true") {
            $("#linenumbers").attr("checked", "checked");
        } else {
            $("#linenumbers").removeAttr("checked");
        }
    });
    $('.pane').hide();

    var unbindChange = function () {
        $(".ratio-input").unbind("change");
        $(".ratio-input").attr("disabled", "disabled");
    };
    var setRatio = function (ratio) {
        crop.setRatio(ratio);
    };
    $("#ratio-none").live('change', function () {
        setRatio(0);
        unbindChange();
    });
    $("#ratio-keep").live('change', function () {
        setRatio(crop.aspectRatio);
        unbindChange();
    });
    $("div.imageCropDialog #ratio-set").live('change', function () {
        $(".ratio-input").removeAttr("disabled");
        setRatio(parseInt($(".ratio-input:eq(0)").val()) / parseInt($(".ratio-input:eq(1)").val()));
        $(".ratio-input").bind("change", function () {
            setRatio(parseInt($(".ratio-input:eq(0)").val()) / parseInt($(".ratio-input:eq(1)").val()));
        });
    });
    $(".crop-width").live('change', function () {
        crop.setSelect([parseInt($(this).val()) / crop.scaleX + crop.cropCoords.x, crop.cropCoords.y2]);
    });
    $(".crop-height").live('change', function () {
        crop.setSelect([crop.cropCoords.x2, parseInt($(this).val()) / crop.scaleY + crop.cropCoords.y]);
    });
    setTimeout(function () {
        author.jscroll = author.editorContainer.JScroll();
        author.loadContent(function (data) {
            $("ul.ribbon").show(); $("body > ul").show();
            author.jscroll.init({ json: data });
            author.jscroll.loadItems();
        });
    }, 500);
});
$(function () {
    $('#mathSrc').keyup(function () { doMathSrc(); }).mouseup(function () { doMathSrc(); }).focusout(function () {
        $('#previewMath').hide();
    });
    $('#insMath').mousedown(function () {
        var sid = author.snippetId;
        if (sid == undefined) {
            alert('Select a text snippet where you want the equation to paste in.');
        } else {
            $('div.editableByTexxtoor[data-item="' + sid + '"]').htmlarea('pasteEquation', doMathSrc(false));
            author.deferredSave($('div.editableByTexxtoor[data-item="' + sid + '"]'));
            $('#previewMath').hide();
            $('#mathSrc').val('');
            $('.equationPane').parent().find('li:first a').click();
            $('.equationPane').hide();
        }
        return false;
    });
    $('li.equationPane span[title]').click(function () {
        $('#mathSrc').val($('#mathSrc').val() + $(this).html());
        doMathSrc();
    });
    var ents_ = { nwarr: '\u2196', swarr: '\u2199' };
    function doMathSrc(paste) {
        $('#mathSrc').focus();
        $('#mathSrc').val().length > 0 ? $('#previewMath').show() : $('#previewMath').hide();
        var srcE = $('#mathSrc')[0],
		ms = srcE.value.replace(/&([-#.\w]+);|\\([a-z]+)(?: |(?=[^a-z]))/ig,
				function (s, e, m) {
				    if (m && (M.macros_[m] || M.macro1s_[m])) return s; // e.g. \it or \sc
				    var t = '&' + (e || m) + ';', res = $('<span>' + t + '</span>').text();
				    return res != t ? res : ents_[e || m] || s;
				}),
		h = ms.replace(/</g, '&lt;');
        if (srcE.value != h) srcE.value = h; // assignment may clear insertion point

        var t;
        try {
            t = M.sToMathE(ms, true);
        } catch (exc) {
            t = String(exc);
        }
        if (paste == undefined) { // preview
            $('#previewMath').empty().append(t);
        }
        //

        if ($('#mathSrc').val().length > 0)
            return '<div contenteditable="false" class="equationContainer">' +
                '<img src="/Content/icons/Editor/delete2_16.png" alt="" onclick="var e = $(this).closest(' + "'.editor'" + '); $(this).parent().remove(); e.focus().click(); author.jscroll.setContentPositionAfterInsert();  author.saveSnippet();" />' + $(t).get(0).outerHTML + '</div>';
    }
});
