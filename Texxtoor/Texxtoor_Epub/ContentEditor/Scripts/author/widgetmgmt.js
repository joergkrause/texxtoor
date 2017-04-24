// Author: Joerg Krause, joerg@augmentedbooks.de

// Assume each widget comes with 3 data attributes:
// data-editor : The editor used to work client side, if empty "contenteditable" is used
// data-style : a style being copied into the iframe
// data-item : the snippet id

function Author(baseUrl, opusid, chapterid, authorUI) {
    this.documentId = opusid,
    this.chapterId = chapterid,
    this.serviceUrl = {
        outerCSS: ['/Content/css/author.css', '/Scripts/author/jqmath/jqmath-0.2.0.css', 'http://fonts.googleapis.com/css?family=UnifrakturMaguntia'],
        updateWidgetContainer: baseUrl + 'UpdateWidgetContainer',
        insertWidget: baseUrl + 'InsertWidget',
        loadContent: baseUrl + 'LoadContent',
        getWidget: baseUrl + 'GetWidget',
        move: baseUrl + 'Move',
        searchSnippetId: baseUrl + 'SearchSnippetId',
        getContentStructure: baseUrl + 'GetContentStructure',
        updateWidgetTools: baseUrl + 'UpdateWidgetTools',
        insertSnippet: baseUrl + 'InsertSnippet',
        deleteSnippet: baseUrl + 'DeleteSnippet',
        relocateAfterSnippetOperation: baseUrl + 'AuthorRoom/' + opusid + '?chapterId=' + chapterid,
        loadComments: baseUrl + 'Comments',
        saveComments: baseUrl + 'Comments',
        saveContent: baseUrl + 'SaveContent',
        tableOfContent: baseUrl + 'Toc'
    },
    this.editorStack = [],
    this.statusBar = null,
    this.authorUI = authorUI,
    this.editorContainer = null
}

Author.prototype = {
    documentId: null,
    chapterId: null,
    snippetId: null,
    lastSnippetId: null,
    editorStack: null,
    serviceUrl: null,
    statusBar: null,
    messageBar: null,
    authorUI: null,
    editorContainer: null,
    isRedirect: false,
    redirectSnippet: null,
    range: null,
    isSaved: false,
    isLoaded: false,
    cropCoords: null,
    keepImageRatio: false,
    imageSize: [],
    contentStructure: null,
    contentData: [],
    direction: 1,
    snippetPosition: 0,
    jsonObj: null,
    jscroll: null,
    sortableIn: 1,
    clipboard: '',
    isSetCaretDown: false,
    isSetCaretUp: false,
    currentCharPos: 0,
    activeSearchElement: {},
    lastSearchPosition: null,
    setStatusBar: function (text, message) {
        if (this.statusBar != null && !message) {
            $(this.statusBar).html(text);
        }
        if (this.messageBar != null && message) {
            $(this.messageBar).html(text);
        }
    },
    prepareEditor: function () {
        var $this = this;
        if ($.browser.opera) {
            document.designMode = "on";
        }
        shortcut.add("Ctrl+S", function () {
            if ($this.snippetId != undefined) { $this.saveSnippet(); }
        });
        // special functions
        $('.editableSection').bind('keydown', function (event) {
            // prevent ENTER and add a paragraph instead
            if (event.which == 13) {
                $this.save(this); // Save current element before adding new one
                $this.insertCommand('text');
                return false;
            }
        });

        // Manage meta Data
        $('.metaDataText').bind('click', function () {
            var srcElement = $(this);
            $this.setSnippetId(srcElement.data("item"));
            var e = $('.metaDataDialog[data-item="' + $this.snippetId + '"]');
            var chapterId = $(e).data('chapter-id');
            $('.metaDataDialog').each(function () {
                if (srcElement.data('item') != $this.snippetId) {
                    srcElement.html("");
                }
            });
            $.ajax({
                url: '/AuthorPortal/Opus/MetaData/' + $this.snippetId + '?chapterId=' + chapterId,
                dataType: 'html',
                success: function (data) {
                    e.html(data);
                    var h = $(srcElement).position().top;
                    e.css('top', h + 'px');
                    e.show();
                    $this.highlightSnippet();
                    $this.setMetaDataRibbonMenu();
                }
            });
        });

        $('.metaDataClose').live('click', function () {
            $(this).parent().hide();
        });
        $(".imagePane input[name='width']").live("change", $this.setImageSize);
        $(".imagePane input[name='height']").live("change", $this.setImageSize);

        var getCaretCharacterOffsetWithin = function (obj) {
            var win = document.defaultView || document.parentWindow;
            var sel, range, preCaretRange, caretOffset = 0;
            if (typeof win.getSelection != "undefined") {
                sel = win.getSelection();
                if (sel.rangeCount) {
                    range = sel.getRangeAt(0);
                    preCaretRange = range.cloneRange();
                    preCaretRange.selectNodeContents(obj[0]);
                    preCaretRange.setEnd(range.endContainer, range.endOffset);
                    caretOffset = preCaretRange.toString().length;
                }
            } else if ((sel = document.selection) && sel.type != "Control") {
                range = document.selection.createRange();
                preCaretRange = document.body.createTextRange();
                preCaretRange.moveToElementText(obj[0]);
                preCaretRange.setEndPoint("EndToEnd", textRange);
                caretOffset = preCaretTextRange.text.length;
            }
            return caretOffset;
        };
        
        $(".snippet-block").each(function () {
            $(this).find(".saveableByTexxtoor").bind('keyup', function (e) {
                if (e.keyCode >= 37 && e.keyCode <= 40) {
                    var newPos = getCaretCharacterOffsetWithin($(this));
                    if ($this.currentCharPos == newPos) {
                        var target;
                        if (e.keyCode == 40 || e.keyCode == 39) {
                            // down
                            target = $(this).parents('.snippet-block').nextAll('.snippet-block:first').find(".saveableByTexxtoor");
                            if (target.length > 0) {
                                $this.findCaretTarget(target, false, true);
                            } else {
                                var el = $(this);
                                $.each($this.jscroll.locations, function (i) {
                                    if (el.data("item") == this.id) {
                                        var id = $this.jscroll.locations[(i + 1) < $this.jscroll.locations.length ? (i + 1) : $this.jscroll.locations.length - 1].id;
                                        $this.snippetId = id;
                                        $this.isSetCaretDown = true;
                                        $this.jscroll.scrollToSnippet(id);
                                    }
                                });
                            }
                        } else {
                            // up
                            target = $(this).parents('.snippet-block').prevAll('.snippet-block:first').find(".saveableByTexxtoor");
                            if (target.length > 0) {
                                $this.findCaretTarget(target, true, false);
                            } else {
                                var el = $(this);
                                $.each($this.jscroll.locations, function (i) {
                                    if (el.data("item") == this.id) {
                                        var id = $this.jscroll.locations[(i - 1) > 0 ? (i - 1) : 0].id;
                                        $this.snippetId = id;
                                        $this.isSetCaretUp = true;
                                        $this.jscroll.scrollToSnippet(id);
                                    }
                                });
                            }
                        }
                    }
                    $this.currentCharPos = newPos;
                    return false;
                }

            });
        });
        $this.isLoaded = true;

    },
    findCaretTarget: function (target, isUp, isDown) {
        var $this = this;
        switch (target.data('editor')) {
            case "ListingEditor":
            case "ImageEditor":
            case "TableEditor":
                target.find('input[data-item="' + target.data('item') + '"]').focus();
                return true;
            case "HtmlEditor":
                var editor = target.find(".editor");
                if (isDown) { $this.setCaretToStart(editor); }
                if (isUp) { $this.setCaretToEnd(editor); }
                return true;
            default:
                if (isDown) { $this.setCaretToStart(target); }
                if (isUp) { $this.setCaretToEnd(target); }
                return true;
        }
        return false;
    },
    setCaretToStart: function (target) {
        if (target[0].createTextRange) {
            var range = target[0].createTextRange();
            range.collapse(true);
            range.moveStart('character', 0);
            range.moveEnd('character', 0);
            range.select();
        }
        else {
            target.focus();
            var sel = window.getSelection();
            var range = sel.getRangeAt(0);
            if ($.browser.mozilla) {
                var nodeLength = sel.anchorNode.childNodes.length;
                var firstNode = nodeLength > 0 ? sel.anchorNode.childNodes[0] : sel.anchorNode;
                range.setStart(firstNode, 0);
                range.setEnd(firstNode, 0);
            } else {
                range.setStart(sel.anchorNode, 0);
                range.setEnd(sel.anchorNode, 0);
            }
            sel.removeAllRanges();
            sel.addRange(range);
        }
    },
    setCaretToEnd: function (target) {
        if (target[0].createTextRange) {
            var range = editor[0].createTextRange();
            range.moveToElementText(target);
            range.collapse(true);
            range.moveStart('character', target[0].innerHTML.length - 1);
            range.moveEnd('character', target[0].innerHTML.length - 1);
            range.select();
        }
        else { 
            target.focus();
            var sel = window.getSelection();
            var range = sel.getRangeAt(sel.rangeCount - 1);
            if ($.browser.mozilla) {
                var nodeLength = sel.anchorNode.childNodes.length;
                var lastNode = nodeLength > 0 ? sel.anchorNode.lastChild : sel.anchorNode;
                console.log(lastNode.length);
                range.setStart(lastNode, lastNode.length);
                range.setEnd(lastNode, lastNode.length);
            } else {
                range.setStart(sel.anchorNode, sel.anchorNode.length);
                range.setEnd(sel.anchorNode, sel.anchorNode.length);
            }
            sel.removeAllRanges();
            sel.addRange(range);
        }
    },
    deferredSave: function (elm) {
        var $this = this;
        if ($this.isSaved) return;
        var id = $(elm).data('item');
        $this.setSnippetId(id);
        var hl = $('.editable_highlight_haschanged[data-item=' + id + ']');
        var ho = $('.editable_highlight[data-item=' + id + ']');
        hl.height(ho.height());
        $this.setStatusBar('Document changed');
        if (!hl.is(':visible')) {
            $this.isSaved = true;
            setTimeout(function () { $this.save($(elm)); hl.hide(); $this.isSaved = false; }, 2000);
        }
        hl.show();
    },
    removeHighlightSnippet: function () {
        var $this = this;
        // remove highlight from all others
        $('div.editable_highlight').each(function () {
            $(this).hide();
        });
    },
    highlightSnippet: function () {
        var $this = this;
        $this.removeHighlightSnippet();
        // add highlight to current
        if ($('#leftbar').is(":visible"))
            $('div.editable_highlight[data-item="' + $this.snippetId + '"]').show();

        $('div.editable_highlight[data-item="' + $this.snippetId + '"]').height($('.editableByTexxtoor[data-item="' + $this.snippetId + '"]').height());
    },
    activateEditor: function (e) {
        var $this = this;
        
        var fn = function () {
            var id = $(this).data("item");
            if (id == undefined) { id = $(this).find('[data-item]').data('item'); }
            activateSnippet(id);
        };
        var activateSnippet = function (id, element) {
            if ($this.lastSnippetId == id) return;
            $this.setSnippetId(id);
            $this.jsonObj = null;
            $this.jsonObj = $.parseJSON($("input[name='properties-" + id + "']").val());
            // remember snippet for all subsequent actions
            $('.miniToolSet').each(function () {
                $(this).hide();
            });
            $('.metaDataDialog').hide();
            $this.setMetaDataRibbonMenu();
            $this.updateWidgetTools();
            // get toolset for this particular snippet
            $('.miniToolSet[data-item="' + $this.snippetId + '"]').show();
            $this.highlightSnippet();
            $this.lastSnippetId = $this.snippetId;
        };
        switch ($(e).data("editor")) {
            case "TableEditor":
            case "HtmlEditor": 
                var area = $(e).htmlarea({
                    css: $this.serviceUrl.outerCSS,
                    onActivate: function (element, id) { activateSnippet(id, element); },
                    onSave: function (elm) { $this.deferredSave(elm); }
                });
                break;
            case "ListingEditor":
                $(e).click(fn).keydown(fn).focus(fn);
                var lnVal = $("input[name='linenumbers-" + $(e).data("item") + "']").val();
                var code = CodeMirror.fromTextArea($(e).find('textarea').get(0), {
                    lineNumbers: (lnVal == undefined ? false : lnVal.toLowerCase() === 'true'),
                    mode: $("input[name='language-" + $(e).data("item") + "']").val(),
                    matchBrackets: true,
                    onChange: function () {
                        $this.deferredSave($(e));
                    }
                });
                $this.editorStack[$(e).data("item")] = code;
                break;
            default:
                $(e).click(fn).keydown(fn).focus(fn);
                break;
        }
    },
    setImageSize: function () {
        var $this = this;
        var width = $(".imagePane input[name='width']");
        var height = $(".imagePane input[name='height']");
        $(".editableImage[data-item=" + $this.snippetId + "]").find(".imageEditor input:eq(1)").val(width);
        $(".editableImage[data-item=" + $this.snippetId + "]").find(".imageEditor input:eq(2)").val(height);
    },
    // Service Calls
    updateWidgetContainer: function () {

        var $this = this;
        $this.authorUI.createLinkDialogs();
        $this.isLoaded = false;
        $this.setStatusBar('Updating document...');
        $(".loader-msg").text("Updating document");
        $(".loader-layout").fadeIn(200, "linear");
        $.ajax({
            url: $this.serviceUrl.updateWidgetContainer,
            data: {
                id: $this.documentId,
                chapterId: $this.chapterId
            },
            type: "POST",
            cache: false,
            dataType: "html",
            success: function (data) {
                $this.setStatusBar('Wait...');
                $("body > ul, ul.ribbon").fadeIn(200, "linear");
                $("#body").fadeIn(400, function () {
                    $this.editorContainer.html(data);
                    if ($this.isRedirect) {
                        $this.isRedirect = false;
                        $(document).scrollTop($('.editableByTexxtoor[data-item="' + $this.redirectSnippet + '"]').offset().top - (window.innerHeight / 2));
                    }
                    $this.prepareEditor();
                    $('#documentName').html($('h1.editableByTexxtoor').text());
                    $(".loader-layout").fadeOut(200, "linear");
                    $this.editorContainer.JScroll();
                });
                $this.setStatusBar('Updated...');
            },
            error: function (data) {
                $this.setStatusBar('Error: ' + data);
                $(".loader-layout").fadeOut(200, "linear");
                setTimeout('', 4000);
            }
        });

    },
    // insert a single snippet
    insertWidget: function (data) {
        var $this = this;
        $this.setStatusBar('Updating document...');
        $.ajax({
            url: $this.serviceUrl.insertWidget,
            data: {
                id: $this.documentId,
                chapterId: $this.chapterId,
                snippetId: data.id
            },
            type: "POST",
            cache: false,
            dataType: "html",
            success: function (html) {
                $this.setStatusBar('Wait...');
                $this.loadContent(function (data) {
                    $this.snippetId = data.id;
                    $this.jscroll.loadData({ json: data });
                    $this.jscroll.scrollToSnippet($this.snippetId);
                });
            },
            error: function (html) {
                $this.setStatusBar('Error: ' + html);
            }
        });
    },
    loadContent: function (callback) {
        var $this = this;
        $this.isLoaded = false;
        $.ajax({
            url: $this.serviceUrl.getContentStructure,
            data: {
                id: $this.documentId,
                chapterId: $this.chapterId
            },
            type: "POST",
            cache: false,
            dataType: "json",
            success: function (data) {
                $this.contentStructure = data;
                callback(data);
            }
        });
    },
    updateWidgetTools: function (id) {
        // called after each section/editor activation to see what sections can be added
        var $this = this;
        $.ajax({
            url: $this.serviceUrl.updateWidgetTools,
            data: {
                id: $this.documentId,
                chapterId: $this.chapterId,
                snippetId: $this.snippetId
            },
            type: "GET",
            cache: false,
            dataType: "json",
            success: function (data) {
                //!(data.S1) ? $(".ribbon-button[data-action='section']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='section']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.S2) ? $(".ribbon-button[data-action='subsection']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='subsection']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.S3) ? $(".ribbon-button[data-action='subsubsection']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='subsubsection']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.S4) ? $(".ribbon-button[data-action='subsubsubsection']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='subsubsubsection']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Bold) ? $(".ribbon-button[data-action='bold']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='bold']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Italic) ? $(".ribbon-button[data-action='italic']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='italic']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Underline) ? $(".ribbon-button[data-action='underline']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='underline']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Sub) ? $(".ribbon-button[data-action='sub']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='sub']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Sup) ? $(".ribbon-button[data-action='sup']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='sup']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Ul) ? $(".ribbon-button[data-action='unsortedlist']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='unsortedlist']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Ol) ? $(".ribbon-button[data-action='sortedlist']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='sortedlist']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Increase) ? $(".ribbon-button[data-action='l']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='l']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Decrease) ? $(".ribbon-button[data-action='r']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='r']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Up) ? $(".ribbon-button[data-action='u']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='u']").removeAttr("disabled").fadeTo("100", "1.0");
                !(data.Down) ? $(".ribbon-button[data-action='d']").attr("disabled", "disabled").fadeTo("100", "0.4") : $(".ribbon-button[data-action='d']").removeAttr("disabled").fadeTo("100", "1.0");

            },
            error: function (data) {
            }
        });
    },
    formatCommand: function (src, cmd, args) {
        var $this = this;
        if ($this.snippetId) {
            if ($(src).attr('disabled') != "disabled") {
                $('#sn-' + $this.snippetId).htmlarea(cmd, args);
                //$this.saveSnippet();
            }
        } else {
            alert('Cannot invoke this command, please select text snippet\n\nCommand: ' + cmd);
        }
    },
    findCommand: function () {
        var $this = this;
        $this.clearFR();
        if (!$this.fr.is(":visible")) {
            $this.fr.find("#replace").hide();
            $this.fr.slideDown(100,function () {
                    $this.fr.find("#find input").focus();
                });
            $this.highlightText();
        } else if ($this.fr.is(":visible") && $this.fr.find("#replace").is(":visible")) {
            $this.fr.slideUp(100, function () {
                $this.fr.find("#replace").hide();
                $this.fr.slideDown(100, function () {
                    $this.fr.find("#find input").focus();
                });
                $this.highlightText();
            });
        } else {
            $(".editor").removeHighlight();
            $this.fr.slideUp(100, function () {
                $this.fr.find("#replace").show(function () {
                    $this.fr.find("#find input").focus();
                });
            });
        }
    },
    clearFR: function () {
        var $this = this;
        $(".editor, .CodeMirror, .editableSection").removeHighlight();
        $this.fr = $(".fr-wrapper");
        $this.fr.find("#find input").val('');
        $this.fr.find("#replace input").val('');
        $this.fr.find("#find input").unbind("keyup");
        $this.fr.find("#f-close, #f-next, #f-prev, #r-replace").unbind("click");
    },
    replaceCommand: function () {
        var $this = this;
        $this.clearFR();
        if ($this.fr.is(":visible") && !$this.fr.find("#replace").is(":visible")) {
            $this.fr.slideUp(100, function () {
                $this.fr.find("#replace").show();
                $this.fr.slideDown(200, function () {
                    $this.fr.find("#find input").focus();
                });
                $this.highlightText();
            });

        } else if (!$this.fr.is(":visible")) {
            $this.fr.slideDown(100, function () {
                $this.fr.find("#find input").focus();
            });
            $this.highlightText();
        } else {
            $(".editor, .CodeMirror, .editableSection").removeHighlight();
            $this.fr.slideUp(100, function () {
                $this.fr.find("#replace").show();
                $this.fr.find("#find input").focus();
            });
        }
    },
    highlightText: function () {
        var $this = this;
        $this.highlights = null;
        $this.hIndex = 0;
        if ($this.snippetId == null)
            $this.setSnippetId(parseInt($(".snippet-block:first").find(".editableByTexxtoor").attr("data-item")));

        $this.fr.find("#find input").bind("keyup paste", function () {
            var i = this;
            clearTimeout($this.timeoutId);
            $this.timeoutId = setTimeout(function () {
                $this.highlights = $this.search(i);
                $this.hIndex = 0;
                $this.activeSearchElement = { snippetId: $this.snippetId, position: $this.hIndex };
                if ($this.highlights == undefined) return;
                if ($this.highlights.length == 0) {
                    $this.searchSnippetId($("#find input").val());
                    return;
                }
                $this.setSnippetId(parseInt($("span.highlight:first").parents(".editableByTexxtoor").attr("data-item")));
                $this.activeSearchElement = { snippetId: $this.snippetId, position: $this.hIndex };
                $this.setHighlightStyle();
            }, 200);
        });
        $this.fr.find("#r-replace").bind('click', function () {
            if ($(".loader-layout").is(":visible") && !$this.jscroll.isLoaded) return;
            if ($this.highlights.length == 0) return;
            $($this.highlights[$this.hIndex]).text($this.fr.find("#replace input").val());
            var e = $($this.highlights[$this.hIndex]).closest(".editor, .CodeMirror").parent();
            e = e.length > 0 ? e : $($this.highlights[$this.hIndex]).closest('.editableSection');
            $this.deferredSave(e);
            $(".editor, .CodeMirror, .editableSection").removeHighlight();
        });
        $this.fr.find("#f-next").bind('click', function () {
            if ($(".loader-layout").is(":visible")) return;
           // $this.direction = 0;
            if (!$('#sn_block-' + $this.activeSearchElement.snippetId).is(":visible")) {
                $this.jscroll.scrollToSnippet($this.activeSearchElement.snippetId);
                return;
            }
            $this.direction = 1;
            if ($this.hIndex < $('#sn_block-' + $this.activeSearchElement.snippetId).find("span.highlight").length - 1) {
                $this.hIndex++;
                $this.activeSearchElement.position = $this.hIndex;
                $this.setHighlightStyle();
            }
            else {
                var highlights = $('#sn_block-' + $this.activeSearchElement.snippetId).nextAll(".snippet-block").find("span.highlight");
                if (highlights.length != 0) {
                    $this.hIndex = 0;
                    $this.activeSearchElement.position = $this.hIndex;
                    $this.activeSearchElement.snippetId = $(highlights[0]).parents(".editableByTexxtoor").attr("data-item");
                    $this.setHighlightStyle();
                } else {
                    $this.searchSnippetId($("#find input").val());
                }
            }
        });
        $this.fr.find("#f-prev").bind('click', function () {
            if ($(".loader-layout").is(":visible") && !$this.jscroll.isLoaded) return;
           // $this.direction = 0;
            if (!$('#sn_block-' + $this.activeSearchElement.snippetId).is(":visible")) {
                $this.jscroll.scrollToSnippet($this.activeSearchElement.snippetId);
                return;
            }
            $this.direction = -1;
            if ($this.hIndex > 0) {
                $this.hIndex--;
                $this.activeSearchElement.position = $this.hIndex;
                $this.setHighlightStyle();
            }
            else {
                var highlights = $('#sn_block-' + $this.activeSearchElement.snippetId).prevAll(".snippet-block").find("span.highlight");
                if (highlights.length != 0) {
                    $this.hIndex = $(highlights[0]).parents(".snippet-block").find("span.highlight").length - 1;
                    $this.activeSearchElement.position = $this.hIndex;
                    $this.activeSearchElement.snippetId = $(highlights[0]).parents(".editableByTexxtoor").attr("data-item");
                    $this.setHighlightStyle();
                } else {
                    $this.searchSnippetId($("#find input").val());
                }
            }
        });
        $this.fr.find("#f-close").bind('click', function () {
            $(".editor, .CodeMirror, .editableSection").removeHighlight();
            $this.fr.slideUp(100, function () {
                $this.clearFR();
            });
        });
    },
    search: function (e) {
        var $this = this;
        if ($(e).val().length == 0) {
            $(".editor, .CodeMirror, .editableSection").removeHighlight();
            return null;
        }
        $(".editor, .CodeMirror, .editableSection").removeHighlight().highlight($(e).val());
        return $(".editor, .CodeMirror, .editableSection").find(".highlight");
    },
    searchSnippetId: function (searchValue) {
        var $this = this;
        $.ajax({
            url: $this.serviceUrl.searchSnippetId,
            data: {
                id: $this.documentId,
                chapterId: $this.chapterId,
                snippetId: $this.snippetId,
                value: searchValue,
                direction: $this.direction
            },
            type: "POST",
            cache: false,
            dataType: "json",
            success: function (data) {
                if (data.Id == 0) return;
                $this.setSnippetId(data.Id);
                $this.activeSearchElement.snippetId = data.Id;
                $this.snippetId = data.Id;
                $this.jscroll.scrollToSnippet(data.Id);
            },
            error: function (data) {
            }
        });
    },
    setHighlightStyle: function () {
        var $this = this;
        $this.highlights = $this.search($("#find input"));
        if (typeof $this.highlights === 'undefined' || $this.highlights.length == 0) {
            return;
        }
        $this.direction = 0;
        $($this.highlights).css('background', 'yellow').removeClass("active");
        var active = $('#sn_block-' + $this.activeSearchElement.snippetId).find("span.highlight").eq($this.activeSearchElement.position);
        if (active.length == 0) return;
        active.css('background', '#FF7200').addClass("active");
        $this.setSnippetId(parseInt($("span.highlight.active").parents(".editableByTexxtoor").attr("data-item")));
        if (active.position().top + active.height() + active.parents(".snippet-block").position().top > $this.editorContainer.height()) {
            $this.direction = 0;
            $this.jscroll.pageDown();
        } else if (active.parents(".snippet-block").position().top + active.position().top < 0) {
            $this.direction = 0;
            $this.jscroll.pageUp();
        }
    },
    setListingEditorOption: function (cmd, args) {
        var $this = this;
        $('input[name=' + cmd + '-' + author.snippetId + ']').val(args);
        var editor = $this.editorStack[$this.snippetId];
        switch (cmd) {
            case "language": editor.setOption('mode', args);
                $('input[name=syntaxhighlight-' + author.snippetId + ']').val(true);
                $("#syntaxhighlight").attr("checked", "checked");
                break;
            case "syntaxhighlight": editor.setOption('mode', args == false ? 'text' : $("input[name='language-" + author.snippetId + "']").val());
                break;
            case "linenumbers": editor.setOption('lineNumbers', args);;
                break;
        }
        
        $this.contentData[author.chapterId][$this.snippetId] = null;
        $this.saveSnippet();
    },
    setSidebarType: function (obj) {
        var header = $(".editableByTexxtoor[data-item='" + this.snippetId + "'] .editor").children("header");
        header.text($(obj).text());
        $(obj).find("input").val() == 5 ? header.attr("contenteditable", 'true') : header.attr("contenteditable", 'false');
        $(".editableByTexxtoor[data-item='" + this.snippetId + "']").children("input[type='hidden']").val($(obj).find("input").val());
    },
    insertTerm: function (type, v, title) {
        var $this = this;
        if (!$this.snippetId) return;
        $('#sn-' + $this.snippetId).focus().click();
        var selection = $('#sn-' + $this.snippetId).htmlarea('getSelectedHTML');
        if (selection.length == 0) {
            selection = title;
        }
        var targetElement = "span";
        // get the element for this term type
        switch (type) {
            case 'abbreviation':
                targetElement = "abbr";
                break;
            case 'cite':
                targetElement = "cite";
                break;
            case 'idiom':
                targetElement = "em";
                break;
            case 'variables':
                targetElement = "var";
                break;
            case 'definitions':
                targetElement = "def";
                break;
        }
        $('#sn-' + $this.snippetId).htmlarea('pasteHTML', '<' + targetElement + ' contenteditable="false" title="' + title + '" data-item="' + v + '">' + selection + '</' + targetElement + '><span>&nbsp</span>');
    },
    insertCommand: function (cmd, variation, data) {
        var $this = this;
        $this.authorUI.saveScroll();
        $this.saveSnippet();
        var id = $this.snippetId;
        $this.setStatusBar("Snippet inserting");
        $.ajax({
            url: $this.serviceUrl.insertSnippet,
            data: {
                documentId: $this.documentId,
                id: id,
                chapterId: $this.chapterId,
                type: cmd,
                variation: variation,
                data: data
            },
            type: "POST",
            cache: false,
            dataType: "json",
            success: function (data) {
                $this.setStatusBar("Snippet Inserted..." + data.msg);
                if (cmd == "chapter") {
                    $this.chapterId = data.relocateTo;
                    $this.contentData[author.chapterId] = new Array();
                    $this.updateTableOfContent();
                    $this.loadContent(function (data) {
                        $this.jscroll.loadData({ json: data });
                        $this.jscroll.setScrollPosition(0);
                    });
                } else {
                    $this.contentData[author.chapterId] = new Array();
                    $this.snippetId = data.id;
                    $this.loadContent(function (data) {
                        $this.jscroll.loadData({ json: data });
                        $this.jscroll.setScrollPosition($this.jscroll.scrollPosition);
                    });
                }
            },
            error: function (data) {
                $this.setStatusBar("Not inserting...");
                $(".loader-layout").fadeOut(200, "linear");
            }
        });
    },
    figureCommand: function (action, obj) {
        var $this = this;

        switch (action) {
            case "crop":
                if ($(".cmd-button").attr("disable")) {
                    return false;
                }
                crop.setSnippetId($this.snippetId);
                $('div.imageCropDialog  .image-container').find("img.source-img").remove();
                var src = $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src');
                crop.loadImageContainer();
                crop.setImage(src);
                $('div.imageCropDialog, .popup-layout').show();
                crop.release();
                break;
            case "keepsize":
                if ($("#keepsize").attr("disable")) {
                    $("#keepsize").attr("checked", "checked");
                    return false;
                }
                if ($("#keepsize").attr("checked")) {
                    $(".imagePane input[type='text']").spinner('disable');
                    $this.jsonObj.KeepSize = true;
                    if ($this.jsonObj["Crop"] == null) {
                        $this.jsonObj.ImageHeight = $this.jsonObj.OriginalHeight;
                        $(".imagePane input[name='width']").val($this.jsonObj.OriginalWidth);
                        $(".imagePane input[name='height']").val($this.jsonObj.OriginalHeight);
                        $this.jsonObj.ImageWidth = $this.jsonObj.OriginalWidth;
                        setTimeout(function () {
                            var src = $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src');
                            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src', src);
                            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").load(function () {
                                $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").width($this.jsonObj.OriginalWidth);
                                $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").height($this.jsonObj.OriginalHeight);
                            });
                        }, 1000);
                    }
                } else {
                    $(".imagePane input[type='text']").spinner('enable');
                    $this.jsonObj.KeepSize = false;
                }
                $("input[name='properties-" + $this.snippetId + "']").val(JSON.stringify($this.jsonObj));
                $this.saveSnippet();
                break;
            case "setsize":
                if ($this.keepImageRatio) {
                    if (obj != null) {
                        if (obj.attr('name') == 'width') {
                            $(".imagePane input[name='height']").val(Math.round(obj.val() * ($this.jsonObj.ImageHeight / $this.jsonObj.ImageWidth)));
                        } else if (obj.attr('name') == 'height') {
                            $(".imagePane input[name='width']").val(Math.round(obj.val() * ($this.jsonObj.ImageWidth / $this.jsonObj.ImageHeight)));
                        }
                    } else {
                        if ($this.imageSize[0] != $this.jsonObj.ImageWidth) {
                            $(".imagePane input[name='height']").val(Math.round($this.imageSize[0] * ($this.jsonObj.ImageHeight / $this.jsonObj.ImageWidth)));
                        } else if ($this.imageSize[1] != $this.jsonObj.ImageHeight) {
                            $(".imagePane input[name='width']").val(Math.round($this.imageSize[1] * ($this.jsonObj.ImageWidth / $this.jsonObj.ImageHeight)));
                        }
                    }
                    $this.jsonObj.ImageWidth = $(".imagePane input[name='width']").val();
                    $this.jsonObj.ImageHeight = $(".imagePane input[name='height']").val();
                }
                $this.imageSize[0] = $(".imagePane input[name='width']").val();
                $this.imageSize[1] = $(".imagePane input[name='height']").val();

                clearTimeout($this.imgResizeTimeout);
                $this.imgResizeTimeout = setTimeout(function () {
                    $this.jsonObj.ImageWidth = $this.imageSize[0];
                    $this.jsonObj.ImageHeight = $this.imageSize[1];
                    $("input[name='properties-" + $this.snippetId + "']").val(JSON.stringify($this.jsonObj));
                    $this.save($('.saveableByTexxtoor[data-item=' + $this.snippetId + ']'));
                    setTimeout(function () {
                        var src = $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src');
                        $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src', src);
                        $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").load(function () {
                            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").width($this.jsonObj.ImageWidth);
                            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").height($this.jsonObj.ImageHeight);
                        });
                    }, 1000);
                }, 1500);
                break;
            case "setratio":
                if ($("#keepratio1").is(":visible")) {
                    $this.keepImageRatio = false;
                    $("#keepratio1").hide();
                    $("#keepratio2").show();
                } else {
                    $this.keepImageRatio = true;
                    $("#keepratio2").hide();
                    $("#keepratio1").show();
                }
                $this.figureCommand('setsize');

                break;
            case "colors":
                if ($(".cmd-button").attr("disable")) {
                    return false;
                }
                if ($this.jsonObj.Colors == null) {
                    return;
                }
                $('div.imageColorsDialog, .popup-layout').show();
                var color = $this.jsonObj.Colors['TransparentColor'] != null ? '#' + $this.jsonObj.Colors['TransparentColor'] : '#FFFFFF';
                $("#t-color").val(color);

                $('.t-color-picker > div').css('backgroundColor', color);
                $("#t-color").ColorPicker({
                    color: color,
                    onShow: function (colpkr) {
                        $(colpkr).fadeIn(500);
                        return false;
                    },
                    onHide: function (colpkr) {
                        $(colpkr).fadeOut(500);
                        return false;
                    },
                    onChange: function (hsb, hex, rgb) {
                        $('.t-color-picker > div').css('backgroundColor', '#' + hex);
                        $("#t-color").val('#' + hex);
                        $this.jsonObj.Colors['TransparentColor'] = hex;
                    }
                });
                $(".s-value:eq(0)").html($this.jsonObj.Colors['Brightness']);
                $("#b-slider").slider({
                    animate: true, value: $this.jsonObj.Colors['Brightness'], max: 256, min: -256, change: function (event, ui) {
                        $this.jsonObj.Colors['Brightness'] = ui.value;
                    }, slide: function (event, ui) { $(".s-value:eq(0)").html(ui.value); }
                });
                $(".s-value:eq(1)").html($this.jsonObj.Colors['Contrast']);
                $("#c-slider").slider({
                    animate: true, value: $this.jsonObj.Colors['Contrast'], max: 128, min: -128, change: function (event, ui) {
                        $this.jsonObj.Colors['Contrast'] = ui.value;
                    }, slide: function (event, ui) { $(".s-value:eq(1)").html(ui.value); }
                });
                $(".s-value:eq(2)").html($this.jsonObj.Colors['Hue']);
                $("#h-slider").slider({
                    animate: true, value: $this.jsonObj.Colors['Hue'], max: 180, min: -180, change: function (event, ui) {
                        $this.jsonObj.Colors['Hue'] = ui.value;

                    }, slide: function (event, ui) { $(".s-value:eq(2)").html(ui.value); }
                });
                $(".s-value:eq(3)").html($this.jsonObj.Colors['Saturation']);
                $("#s-slider").slider({
                    animate: true, value: $this.jsonObj.Colors['Saturation'], max: 100, min: -100, change: function (event, ui) {
                        $this.jsonObj.Colors['Saturation'] = ui.value;

                    }, slide: function (event, ui) { $(".s-value:eq(3)").html(ui.value); }
                });
                if ($this.jsonObj["Effects"] != null) {
                    var filters = $this.jsonObj["Effects"].split(";");
                    $("#selected-effects").html('');
                    for (var i = 0; i < filters.length - 1; i++) {
                        $("#selected-effects").append($('<div class="ui-state-default">' + filters[i] + '</div>'));
                    }
                }
                $(".connectedSortable").css({ minHeight: $(".connectedSortable:eq(0)").height() });
                $("#available-effects div").draggable({
                    connectToSortable: "#selected-effects",
                    helper: "clone"
                }).disableSelection();
                $("#selected-effects").sortable({
                    connectWith: '#available-effects',
                    forcePlaceholderSize: true,
                    receive: function (e, ui) { $this.sortableIn = 1; },
                    over: function (e, ui) { $this.sortableIn = 1; },
                    out: function (e, ui) { $this.sortableIn = 0; },
                    beforeStop: function (e, ui) {
                        if ($this.sortableIn == 0) {
                            ui.item.remove();
                        }
                    }
                }).disableSelection();

                $('#saveColors').bind('click', function () {
                    $this.jsonObj["Effects"] = "";
                    $("#selected-effects > div").each(function () {
                        $this.jsonObj["Effects"] += $(this).html() + ";";
                    });
                    $("input[name='properties-" + $this.snippetId + "']").val(JSON.stringify($this.jsonObj));
                    $this.saveSnippet();
                    $('div.imageColorsDialog, .popup-layout').hide();
                    $(this).unbind('click');
                    setTimeout(function () {
                        var src = $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src');
                        $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").attr('src', src);
                        $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").load(function () {
                            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").width($this.jsonObj.ImageWidth);
                            $(".editableByTexxtoor[data-item=" + $this.snippetId + "]").find("img").height($this.jsonObj.ImageHeight);
                        });
                    }, 1000);
                });

                break;
            default: break;
        }
    },
    deleteCommand: function () {
        var $this = this;
        $this.authorUI.saveScroll();
        // instantly hide for best user experience
        $this.setStatusBar("Deleting snippet");
        $.ajax({
            url: $this.serviceUrl.deleteSnippet,
            data: {
                documentId: $this.documentId,
                chapterId: $this.chapterId,
                id: $this.snippetId,
                delChildren: $('#delChildren').is(':checked')
            },
            type: "POST",
            cache: false,
            dataType: "json",
            success: function (d) {
                var href = $("#tocpane li:last").prev().find("a").attr('href');
                $this.updateTableOfContent();
                $this.setStatusBar(d.msg);
                // remove physically
                $this.loadContent(function (data) {
                    $this.jscroll.loadData({ json: data });
                    $this.contentData[author.chapterId] = new Array();
                    $('#sn_block-' + $this.snippetId).fadeOut(200, function () {
                        $('#sn_block-' + $this.snippetId).remove();
                    });
                    if ($this.snippetId == $this.chapterId) {
                        $this.setCurrentChapter(href);
                        $this.jscroll.setScrollPosition(0);
                        return;
                    }
                    for (var i = 0; i < d.children.length; i++) {
                        $('#sn_block-' + d.children[i]).fadeOut(200, function () {
                            $(this).remove();
                        });
                    }
                    $this.jscroll.setScrollPosition($this.jscroll.scrollPosition);
                    
                });

            },
            error: function (data) {
                $this.setStatusBar("Not deleted..." + data.msg);
                // let's reappear to show user that something went wrong
            }
        });
    },
    setMetaData: function (obj) {
        var $this = this;
        if ($this.snippetId == null) {
            $(obj).attr("checked", false);
            return;
        }
        var index = $(obj).closest("div").index();
        var itemIndex = 0;

        if ($(obj).is("li")) {
            itemIndex = $(obj).index();
        } else {
            itemIndex = $(obj).closest("li").index();
        }
        var item = $('.metaDataDialog[data-item="' + $this.snippetId + '"]').find("table tr:eq(1) td:eq(" + index + ") input:eq(" + itemIndex + ")");
        item.attr("checked", "checked");
        $(obj).is("li") ? $(obj).find("input").attr("checked", "checked") : $(obj).attr("checked", "checked");
    },
    setMetaDataRibbonMenu: function () {
        var $this = this;
        $('.metaDataDialog[data-item="' + $this.snippetId + '"]').find("table tr:eq(1) input").each(function (index) {
            if ($(this).attr("checked") == "checked") {
                $("li.sem:eq(" + index + ") input").attr("checked", $(this).attr("checked"));
            } else {
                $("li.sem:eq(" + index + ") input").removeAttr("checked");
            }
        });
    },
    move: function (action, obj) {
        var $this = this;
        if (action == null) {
            if (!$(obj).attr('href')) return;
            var arr = obj.href.split("=");
            for (var i = 1; i < arr.length - 1; i++) {
                arr[i] = arr[i].split("&")[0];
            }
            $this.chapterId = parseInt(arr[1]);
            $this.snippetId = parseInt(arr[2]);
            action = arr[3];
        }
        $this.authorUI.saveScroll();
        $this.setStatusBar("Moving snippet");
        $.ajax({
            url: $this.serviceUrl.move,
            type: "GET",
            data: {
                id: $this.documentId,
                chapterId: $this.chapterId,
                sectionId: $this.snippetId,
                move: action
            },
            cache: false,
            success: function (data) {
                $this.contentData[author.chapterId] = new Array();
                $this.loadContent(function (data) {
                    $this.jscroll.loadData({ json: data });
                    $this.jscroll.scrollToSnippet($this.snippetId);
                });
            },
            error: function (data) {
                $this.setStatusBar(data);
            }
        });
    },
    showPopup: function (action) {
        var $this = this;
        $this.range = $('#sn-' + $this.snippetId).htmlarea('getRange');
        var html = $('#sn-' + $this.snippetId).htmlarea('getSelectedHTML');
        if (html.length > 0)
            $('div.' + action + 'LinkDialog .caption input').val(html);
        else
            $('div.' + action + 'LinkDialog #il-tree a').bind('click', function () {
                $('div.' + action + 'LinkDialog .caption input').val($.trim($(this).text()));
            });

        $('div.' + action + 'LinkDialog, .popup-layout').show();
    },
    addLink: function () {
        var $this = this;
        var node = $("#il-tree").jstree('get_selected');
        var root = node.parents('li ul');
        root = root.eq(root.length - 1).parent();
        var snippetId = node.children("a").data('value');
        var chapterId = root.children("a").data('value');
        chapterId = chapterId ? chapterId : snippetId;
        var html = $('.dialog-popup .caption input').val();
        html = '<a class="innerLink" href="#" data-snippet="' + snippetId + '" data-chapter="' + chapterId + '">' + html + '</a>';
        if ($.browser.msie)
            $this.range.pasteHTML(html);
        else {
            $this.range.deleteContents();
            $this.range.insertNode($(html)[0]);
        }
        $this.range.collapse(true);
        $this.saveSnippet();
        $('.dialog-popup').find('a').unbind('click');
        $('.dialog-popup').hide();
        $('.popup-layout').hide();
    },
    setCurrentChapter: function (href) {
        var $this = this;
        $this.chapterId = href.substring(href.lastIndexOf("=") + 1, href.lenght);
        $this.authorUI.destroyScrollPosition();
        $this.loadContent(function (data) {
            $this.jscroll.loadData({ json: data });
            $this.jscroll.setScrollPosition(0);
        });
        $('.orb li ul').fadeOut('slow');

    },
    saveSnippet: function () {
        var $this = this;
        var elm = $('.saveableByTexxtoor[data-item=' + $this.snippetId + ']');
        $this.save(elm);
    },
    save: function (e) {
        var $this = this;
        var elm = $(e);
        var content;
        $(".editor, .CodeMirror, .editableSection").removeHighlight();
        $this.authorUI.saveScroll();
        switch (elm.data('editor')) {
            case "HtmlEditor":
                content = $(elm).htmlarea('toHtmlString');
                break;
            case "ListingEditor":
                content = $this.editorStack[$this.snippetId].getValue();
                break
            case "ImageEditor":
                content = $(elm).find('input').val();
                break;
            default:
                content = elm.html();
                break;
        }
        var id = elm.attr('data-item');
        $(".fakeDiv").remove();
        if (!id) {
            return false;
        }
        $.ajax({
            url: $this.serviceUrl.saveContent,
            data: {
                id: id,
                documentId: $this.documentId,
                content: content,
                form: $('form').serialize()
            },
            type: "POST",
            cache: false,
            dataType: "json",
            success: function (data) {
                if (data.sectionRefresh) {
                    $('#chapterTitleInToolSet').html(data.sectionRefresh);
                } 
                $this.contentData[$this.chapterId][$this.snippetId] = null;
                $this.setStatusBar('Document saved');
                if ($(".fr-container").is(":visible")) {
                    $this.setHighlightStyle();
                }
            },
            error: function (data) {
                $this.setStatusBar("Something went wrong..." + data.msg);
            }
        });

    },
    updateTableOfContent: function () {
        var $this = this;
        $.ajax({
            url: $this.serviceUrl.tableOfContent,
            data: { id: $this.documentId },
            type: 'GET',
            cache: false,
            datatype: 'html',
            success: function (data) {
                $('#tocpane').find('li').remove();
                $('#tocpane').find('> div').after(data);
            }
        });
    },
    saveComments: function (type, id) {
        var $this = this;
        $this.setSnippetId(id);
        var txt = $('#' + type + 'comment' + '-' + $this.snippetId).val();
        $.ajax({
            url: $this.serviceUrl.saveComments,
            data: {
                id: $this.snippetId,
                comment: txt,
                type: type
            },
            type: "POST",
            cache: false,
            success: function (data) {
                $this.loadComments();
            },
            error: function (data) {
                $this.setStatusBar("Not inserting...");
            }
        });
    },
    redirectToSnippet: function (chapter, id) {
        var $this = this;
        $this.isRedirect = true;
        $this.chapterId = chapter;
        $this.loadContent(function (data) {
            $this.jscroll.loadData({ json: data });
            $this.jscroll.scrollToSnippet(id);
        });

    },
    loadComments: function () {
        var $this = this;
        $this.authorUI.hideCommentDialogs();
        $.ajax({
            url: $this.serviceUrl.loadComments,
            data: { id: $this.snippetId },
            type: "GET",
            cache: false,
            dataType: "html",
            success: function (data) {
                var e = $('.commentDialog[data-item="' + $this.snippetId + '"]');
                // we need to reposition the popup if it is at the bottom of the page
                var h = e.position().top;
                var dh = $(document).height();
                if (dh - h < 200) {
                    e.css('top', (dh - 200) + 'px');
                }
                e.html(data);
                e.toggle();
                $('.commentAccordion').accordion('destroy');
                $('.commentAccordion').accordion();
                $('.commentClose').bind('click', function () {
                    $this.authorUI.hideCommentDialogs();
                });
            },
            error: function (data) {
                $this.setStatusBar("An error occured loading comments...");
            }
        });
    },
    cleanOnPaste: function () {
    },
    setSnippetId: function (id) {
        if (isNaN(id)) return;
        var $this = this;
        $this.setStatusBar(id, true);
        $this.snippetId = id;
        //$this.updateWidgetTools();
        $this.highlightSnippet();
    }
}