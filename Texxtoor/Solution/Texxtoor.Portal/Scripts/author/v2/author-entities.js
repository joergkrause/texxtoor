var AUTHOR = (function (my) {
  // insert entities

  //Thanks to tinymce's (http://tinymce.moxiecode.com) charmap implementation for
  //character definitions and layout inspiration

  //Thanks to WYMeditor https://gist.github.com/winhamwr/4611209

  //Wrap
  my.specialChars = function (options) {
      var specialChars = new SpecialChars(options, this);
      this._specialChars = specialChars;

      return specialChars;
    };

  my.charDialog = function (index) {
      var sc = this._specialChars;
      sc._wdialog = window;
      var options = sc._options;

      $dialog = jQuery('#special_chars_dialog');
      if (sc._options.dialogClass.length > 0) {
        $dialog.addClass(options.dialogClass);
      }
      $dialog.html(sc.getCharMapHtml());

      // Handle dialog click event
      var chop_len = "charmap_".length
      $dialog.find(sc._options.charsSelector).click(function (e) {
        var target = $(e.target);

        if (target.hasClass('wym_add_special_char')) {
          var charmap_id = target.attr('id');
          var character = charmap_id.substring(chop_len);
          sc.insertChar(character);

          return false;
        }
        window.close();
      });
    }

    function SpecialChars(options, wym) {

      this.CHARS = {
        nbsp: ['&nbsp;', '&#160;', 'no-break space'],
        amp: ['&amp;', '&#38;', 'ampersand'],
        quot: ['&quot;', '&#34;', 'quotation mark'],
        // finance
        cent: ['&cent;', '&#162;', 'cent sign'],
        euro: ['&euro;', '&#8364;', 'euro sign'],
        pound: ['&pound;', '&#163;', 'pound sign'],
        yen: ['&yen;', '&#165;', 'yen sign'],
        // signs
        copy: ['&copy;', '&#169;', 'copyright sign'],
        reg: ['&reg;', '&#174;', 'registered sign'],
        trade: ['&trade;', '&#8482;', 'trade mark sign'],
        permil: ['&permil;', '&#8240;', 'per mille sign'],
        micro: ['&micro;', '&#181;', 'micro sign'],
        middot: ['&middot;', '&#183;', 'middle dot'],
        bull: ['&bull;', '&#8226;', 'bullet'],
        hellip: ['&hellip;', '&#8230;', 'three dot leader'],
        prime: ['&prime;', '&#8242;', 'minutes / feet'],
        Prime: ['&Prime;', '&#8243;', 'seconds / inches'],
        sect: ['&sect;', '&#167;', 'section sign'],
        para: ['&para;', '&#182;', 'paragraph sign'],
        szlig: ['&szlig;', '&#223;', 'sharp s / ess-zed'],
        // quotations
        lsaquo: ['&lsaquo;', '&#8249;', 'single left-pointing angle quotation mark'],
        rsaquo: ['&rsaquo;', '&#8250;', 'single right-pointing angle quotation mark'],
        laquo: ['&laquo;', '&#171;', 'left pointing guillemet'],
        raquo: ['&raquo;', '&#187;', 'right pointing guillemet'],
        lsquo: ['&lsquo;', '&#8216;', 'left single quotation mark'],
        rsquo: ['&rsquo;', '&#8217;', 'right single quotation mark'],
        ldquo: ['&ldquo;', '&#8220;', 'left double quotation mark'],
        rdquo: ['&rdquo;', '&#8221;', 'right double quotation mark'],
        sbquo: ['&sbquo;', '&#8218;', 'single low-9 quotation mark'],
        bdquo: ['&bdquo;', '&#8222;', 'double low-9 quotation mark'],
        lt: ['&lt;', '&#60;', 'less-than sign'],
        gt: ['&gt;', '&#62;', 'greater-than sign'],
        le: ['&le;', '&#8804;', 'less-than or equal to'],
        ge: ['&ge;', '&#8805;', 'greater-than or equal to'],
        ndash: ['&ndash;', '&#8211;', 'en dash'],
        mdash: ['&mdash;', '&#8212;', 'em dash'],
        macr: ['&macr;', '&#175;', 'macron'],
        oline: ['&oline;', '&#8254;', 'overline'],
        curren: ['&curren;', '&#164;', 'currency sign'],
        brvbar: ['&brvbar;', '&#166;', 'broken bar'],
        uml: ['&uml;', '&#168;', 'diaeresis'],
        iexcl: ['&iexcl;', '&#161;', 'inverted exclamation mark'],
        iquest: ['&iquest;', '&#191;', 'turned question mark'],
        circ: ['&circ;', '&#710;', 'circumflex accent'],
        tilde: ['&tilde;', '&#732;', 'small tilde'],
        deg: ['&deg;', '&#176;', 'degree sign'],
        minus: ['&minus;', '&#8722;', 'minus sign'],
        plusmn: ['&plusmn;', '&#177;', 'plus-minus sign'],
        divide: ['&divide;', '&#247;', 'division sign'],
        frasl: ['&frasl;', '&#8260;', 'fraction slash'],
        times: ['&times;', '&#215;', 'multiplication sign'],
        sup1: ['&sup1;', '&#185;', 'superscript one'],
        sup2: ['&sup2;', '&#178;', 'superscript two'],
        sup3: ['&sup3;', '&#179;', 'superscript three'],
        frac14: ['&frac14;', '&#188;', 'fraction one quarter'],
        frac12: ['&frac12;', '&#189;', 'fraction one half'],
        frac34: ['&frac34;', '&#190;', 'fraction three quarters'],
        // math / logical
        fnof: ['&fnof;', '&#402;', 'function / florin'],
        int: ['&int;', '&#8747;', 'integral'],
        sum: ['&sum;', '&#8721;', 'n-ary sumation'],
        infin: ['&infin;', '&#8734;', 'infinity'],
        radic: ['&radic;', '&#8730;', 'square root'],
        sim: ['&sim;', '&#8764;', false, 'similar to'],
        cong: ['&cong;', '&#8773;', false, 'approximately equal to'],
        asymp: ['&asymp;', '&#8776;', 'almost equal to'],
        ne: ['&ne;', '&#8800;', 'not equal to'],
        equiv: ['&equiv;', '&#8801;', 'identical to'],
        isin: ['&isin;', '&#8712;', false, 'element of'],
        notin: ['&notin;', '&#8713;', false, 'not an element of'],
        ni: ['&ni;', '&#8715;', false, 'contains as member'],
        prod: ['&prod;', '&#8719;', 'n-ary product'],
        and: ['&and;', '&#8743;', false, 'logical and'],
        or: ['&or;', '&#8744;', false, 'logical or'],
        not: ['&not;', '&#172;', 'not sign'],
        cap: ['&cap;', '&#8745;', 'intersection'],
        cup: ['&cup;', '&#8746;', false, 'union'],
        part: ['&part;', '&#8706;', 'partial differential'],
        forall: ['&forall;', '&#8704;', false, 'for all'],
        exist: ['&exist;', '&#8707;', false, 'there exists'],
        empty: ['&empty;', '&#8709;', false, 'diameter'],
        nabla: ['&nabla;', '&#8711;', false, 'backward difference'],
        lowast: ['&lowast;', '&#8727;', false, 'asterisk operator'],
        prop: ['&prop;', '&#8733;', false, 'proportional to'],
        ang: ['&ang;', '&#8736;', false, 'angle'],
        // undefined
        acute: ['&acute;', '&#180;', 'acute accent'],
        cedil: ['&cedil;', '&#184;', 'cedilla'],
        ordf: ['&ordf;', '&#170;', 'feminine ordinal indicator'],
        ordm: ['&ordm;', '&#186;', 'masculine ordinal indicator'],
        dagger: ['&dagger;', '&#8224;', 'dagger'],
        Dagger: ['&Dagger;', '&#8225;', 'double dagger'],
        // alphabetical special chars
        Agrave: ['&Agrave;', '&#192;', 'A - grave'],
        Aacute: ['&Aacute;', '&#193;', 'A - acute'],
        Acirc: ['&Acirc;', '&#194;', 'A - circumflex'],
        Atilde: ['&Atilde;', '&#195;', 'A - tilde'],
        Auml: ['&Auml;', '&#196;', 'A - diaeresis'],
        Aring: ['&Aring;', '&#197;', 'A - ring above'],
        AElig: ['&AElig;', '&#198;', 'ligature AE'],
        Ccedil: ['&Ccedil;', '&#199;', 'C - cedilla'],
        Egrave: ['&Egrave;', '&#200;', 'E - grave'],
        Eacute: ['&Eacute;', '&#201;', 'E - acute'],
        Ecirc: ['&Ecirc;', '&#202;', 'E - circumflex'],
        Euml: ['&Euml;', '&#203;', 'E - diaeresis'],
        Igrave: ['&Igrave;', '&#204;', 'I - grave'],
        Iacute: ['&Iacute;', '&#205;', 'I - acute'],
        Icirc: ['&Icirc;', '&#206;', 'I - circumflex'],
        Iuml: ['&Iuml;', '&#207;', 'I - diaeresis'],
        ETH: ['&ETH;', '&#208;', 'ETH'],
        Ntilde: ['&Ntilde;', '&#209;', 'N - tilde'],
        Ograve: ['&Ograve;', '&#210;', 'O - grave'],
        Oacute: ['&Oacute;', '&#211;', 'O - acute'],
        Ocirc: ['&Ocirc;', '&#212;', 'O - circumflex'],
        Otilde: ['&Otilde;', '&#213;', 'O - tilde'],
        Ouml: ['&Ouml;', '&#214;', 'O - diaeresis'],
        Oslash: ['&Oslash;', '&#216;', 'O - slash'],
        OElig: ['&OElig;', '&#338;', 'ligature OE'],
        Scaron: ['&Scaron;', '&#352;', 'S - caron'],
        Ugrave: ['&Ugrave;', '&#217;', 'U - grave'],
        Uacute: ['&Uacute;', '&#218;', 'U - acute'],
        Ucirc: ['&Ucirc;', '&#219;', 'U - circumflex'],
        Uuml: ['&Uuml;', '&#220;', 'U - diaeresis'],
        Yacute: ['&Yacute;', '&#221;', 'Y - acute'],
        Yuml: ['&Yuml;', '&#376;', 'Y - diaeresis'],
        THORN: ['&THORN;', '&#222;', 'THORN'],
        agrave: ['&agrave;', '&#224;', 'a - grave'],
        aacute: ['&aacute;', '&#225;', 'a - acute'],
        acirc: ['&acirc;', '&#226;', 'a - circumflex'],
        atilde: ['&atilde;', '&#227;', 'a - tilde'],
        auml: ['&auml;', '&#228;', 'a - diaeresis'],
        aring: ['&aring;', '&#229;', 'a - ring above'],
        aelig: ['&aelig;', '&#230;', 'ligature ae'],
        ccedil: ['&ccedil;', '&#231;', 'c - cedilla'],
        egrave: ['&egrave;', '&#232;', 'e - grave'],
        eacute: ['&eacute;', '&#233;', 'e - acute'],
        ecirc: ['&ecirc;', '&#234;', 'e - circumflex'],
        euml: ['&euml;', '&#235;', 'e - diaeresis'],
        igrave: ['&igrave;', '&#236;', 'i - grave'],
        iacute: ['&iacute;', '&#237;', 'i - acute'],
        icirc: ['&icirc;', '&#238;', 'i - circumflex'],
        iuml: ['&iuml;', '&#239;', 'i - diaeresis'],
        eth: ['&eth;', '&#240;', 'eth'],
        ntilde: ['&ntilde;', '&#241;', 'n - tilde'],
        ograve: ['&ograve;', '&#242;', 'o - grave'],
        oacute: ['&oacute;', '&#243;', 'o - acute'],
        ocirc: ['&ocirc;', '&#244;', 'o - circumflex'],
        otilde: ['&otilde;', '&#245;', 'o - tilde'],
        ouml: ['&ouml;', '&#246;', 'o - diaeresis'],
        oslash: ['&oslash;', '&#248;', 'o slash'],
        oelig: ['&oelig;', '&#339;', 'ligature oe'],
        scaron: ['&scaron;', '&#353;', 's - caron'],
        ugrave: ['&ugrave;', '&#249;', 'u - grave'],
        uacute: ['&uacute;', '&#250;', 'u - acute'],
        ucirc: ['&ucirc;', '&#251;', 'u - circumflex'],
        uuml: ['&uuml;', '&#252;', 'u - diaeresis'],
        yacute: ['&yacute;', '&#253;', 'y - acute'],
        thorn: ['&thorn;', '&#254;', 'thorn'],
        yuml: ['&yuml;', '&#255;', 'y - diaeresis'],
        Alpha: ['&Alpha;', '&#913;', 'Alpha'],
        Beta: ['&Beta;', '&#914;', 'Beta'],
        Gamma: ['&Gamma;', '&#915;', 'Gamma'],
        Delta: ['&Delta;', '&#916;', 'Delta'],
        Epsilon: ['&Epsilon;', '&#917;', 'Epsilon'],
        Zeta: ['&Zeta;', '&#918;', 'Zeta'],
        Eta: ['&Eta;', '&#919;', 'Eta'],
        Theta: ['&Theta;', '&#920;', 'Theta'],
        Iota: ['&Iota;', '&#921;', 'Iota'],
        Kappa: ['&Kappa;', '&#922;', 'Kappa'],
        Lambda: ['&Lambda;', '&#923;', 'Lambda'],
        Mu: ['&Mu;', '&#924;', 'Mu'],
        Nu: ['&Nu;', '&#925;', 'Nu'],
        Xi: ['&Xi;', '&#926;', 'Xi'],
        Omicron: ['&Omicron;', '&#927;', 'Omicron'],
        Pi: ['&Pi;', '&#928;', 'Pi'],
        Rho: ['&Rho;', '&#929;', 'Rho'],
        Sigma: ['&Sigma;', '&#931;', 'Sigma'],
        Tau: ['&Tau;', '&#932;', 'Tau'],
        Upsilon: ['&Upsilon;', '&#933;', 'Upsilon'],
        Phi: ['&Phi;', '&#934;', 'Phi'],
        Chi: ['&Chi;', '&#935;', 'Chi'],
        Psi: ['&Psi;', '&#936;', 'Psi'],
        Omega: ['&Omega;', '&#937;', 'Omega'],
        alpha: ['&alpha;', '&#945;', 'alpha'],
        beta: ['&beta;', '&#946;', 'beta'],
        gamma: ['&gamma;', '&#947;', 'gamma'],
        delta: ['&delta;', '&#948;', 'delta'],
        epsilon: ['&epsilon;', '&#949;', 'epsilon'],
        zeta: ['&zeta;', '&#950;', 'zeta'],
        eta: ['&eta;', '&#951;', 'eta'],
        theta: ['&theta;', '&#952;', 'theta'],
        iota: ['&iota;', '&#953;', 'iota'],
        kappa: ['&kappa;', '&#954;', 'kappa'],
        lambda: ['&lambda;', '&#955;', 'lambda'],
        mu: ['&mu;', '&#956;', 'mu'],
        nu: ['&nu;', '&#957;', 'nu'],
        xi: ['&xi;', '&#958;', 'xi'],
        omicron: ['&omicron;', '&#959;', 'omicron'],
        pi: ['&pi;', '&#960;', 'pi'],
        rho: ['&rho;', '&#961;', 'rho'],
        sigmaf: ['&sigmaf;', '&#962;', 'final sigma'],
        sigma: ['&sigma;', '&#963;', 'sigma'],
        tau: ['&tau;', '&#964;', 'tau'],
        upsilon: ['&upsilon;', '&#965;', 'upsilon'],
        phi: ['&phi;', '&#966;', 'phi'],
        chi: ['&chi;', '&#967;', 'chi'],
        psi: ['&psi;', '&#968;', 'psi'],
        omega: ['&omega;', '&#969;', 'omega'],
        // symbols
        alefsym: ['&alefsym;', '&#8501;', false, 'alef symbol'],
        piv: ['&piv;', '&#982;', false, 'pi symbol'],
        real: ['&real;', '&#8476;', false, 'real part symbol'],
        thetasym: ['&thetasym;', '&#977;', false, 'theta symbol'],
        upsih: ['&upsih;', '&#978;', false, 'upsilon - hook symbol'],
        weierp: ['&weierp;', '&#8472;', false, 'Weierstrass p'],
        image: ['&image;', '&#8465;', false, 'imaginary part'],
        // arrows
        larr: ['&larr;', '&#8592;', 'leftwards arrow'],
        uarr: ['&uarr;', '&#8593;', 'upwards arrow'],
        rarr: ['&rarr;', '&#8594;', 'rightwards arrow'],
        darr: ['&darr;', '&#8595;', 'downwards arrow'],
        harr: ['&harr;', '&#8596;', 'left right arrow'],
        crarr: ['&crarr;', '&#8629;', false, 'carriage return'],
        lArr: ['&lArr;', '&#8656;', false, 'leftwards double arrow'],
        uArr: ['&uArr;', '&#8657;', false, 'upwards double arrow'],
        rArr: ['&rArr;', '&#8658;', false, 'rightwards double arrow'],
        dArr: ['&dArr;', '&#8659;', false, 'downwards double arrow'],
        hArr: ['&hArr;', '&#8660;', false, 'left right double arrow'],
        there4: ['&there4;', '&#8756;', false, 'therefore'],
        sub: ['&sub;', '&#8834;', false, 'subset of'],
        sup: ['&sup;', '&#8835;', false, 'superset of'],
        nsub: ['&nsub;', '&#8836;', false, 'not a subset of'],
        sube: ['&sube;', '&#8838;', false, 'subset of or equal to'],
        supe: ['&supe;', '&#8839;', false, 'superset of or equal to'],
        oplus: ['&oplus;', '&#8853;', false, 'circled plus'],
        otimes: ['&otimes;', '&#8855;', false, 'circled times'],
        perp: ['&perp;', '&#8869;', false, 'perpendicular'],
        sdot: ['&sdot;', '&#8901;', false, 'dot operator'],
        lceil: ['&lceil;', '&#8968;', false, 'left ceiling'],
        rceil: ['&rceil;', '&#8969;', false, 'right ceiling'],
        lfloor: ['&lfloor;', '&#8970;', false, 'left floor'],
        rfloor: ['&rfloor;', '&#8971;', false, 'right floor'],
        lang: ['&lang;', '&#9001;', false, 'left-pointing angle bracket'],
        rang: ['&rang;', '&#9002;', false, 'right-pointing angle bracket'],
        loz: ['&loz;', '&#9674;', true, 'lozenge'],
        spades: ['&spades;', '&#9824;', false, 'black spade suit'],
        clubs: ['&clubs;', '&#9827;', 'black club suit'],
        hearts: ['&hearts;', '&#9829;', 'black heart suit'],
        diams: ['&diams;', '&#9830;', 'black diamond suit'],
        ensp: ['&ensp;', '&#8194;', false, 'en space'],
        emsp: ['&emsp;', '&#8195;', false, 'em space'],
        thinsp: ['&thinsp;', '&#8201;', false, 'thin space'],
        zwnj: ['&zwnj;', '&#8204;', false, 'zero width non-joiner'],
        zwj: ['&zwj;', '&#8205;', false, 'zero width joiner'],
        lrm: ['&lrm;', '&#8206;', false, 'left-to-right mark'],
        rlm: ['&rlm;', '&#8207;', false, 'right-to-left mark'],
        shy: ['&shy;', '&#173;', false, 'soft hyphen']
      };


      options = jQuery.extend({

        charsPerRow: 15,

        tdWidth: 20,

        tdHeight: 20,

        dialogFeatures: "menubar=no,titlebar=no,toolbar=no,resizable=no"
            + ",width=330,height=420,top=0,left=0",

        dialogHtml: "<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Strict//EN'"
            + " 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd'>"
            + "<html dir='"
            + WYMeditor.DIRECTION
            + "'><head>"
            + "<link rel='stylesheet' type='text/css' media='screen'"
            + " href='"
            + WYMeditor.CSS_PATH
            + "' />"
            + "<title>"
            + WYMeditor.DIALOG_TITLE
            + "</title>"
            + "<script type='text/javascript'"
            + " src='"
            + WYMeditor.JQUERY_PATH
            + "'></script>"
            + "<script type='text/javascript'"
            + " src='"
            + WYMeditor.WYM_PATH
            + "'></script>"
                + "<script type='text/javascript'"
            + " src='"
            + WYMeditor.BASE_PATH
            + "/plugins/special_chars/jquery.wymeditor.special_chars.js'></script>"
            + "</head>"
            + WYMeditor.DIALOG_BODY
            + "</html>",

        sBodyHtml: "<body class='wym_dialog wym_dialog_special_chars'"
            + " onload='WYMeditor.INIT_SPECIAL_CHARS_DIALOG(" + WYMeditor.INDEX + ")'"
            + "></body>",

        sButtonHtml: "<li class='wym_tools_special_chars'>"
            + "<a name='special_chars' href='#'"
            + " style='background-image:"
            + " url(" + wym._options.basePath + "plugins/special_chars/char.gif)'>"
            + "Insert Symbol"
            + "</a></li>",

        sButtonSelector: "li.wym_tools_special_chars a",

        charsSelector: "div.wym_special_chars",

        charmapTdClass: 'special_char',

        charmapTrClass: 'special_chars',

        dialogClass: 'flora',

        dialogOptions: {
          autoOpen: false,
          position: 'right',
          title: 'Insert a Symbol',
          width: '320px',
          height: '450px'
        }

      }, options);

      this._options = options;
      this._wym = wym;

      this.init();
    };

    SpecialChars.prototype.init = function () {
      var wym = this._wym;
      var sc = this;

      sc._dia_inner_html = '';

      // Add the tool panel button
      jQuery(wym._box).find(
          wym._options.toolsSelector + wym._options.toolsListSelector)
          .append(sc._options.sButtonHtml);

      // Handle tool button click
      jQuery(wym._box).find(sc._options.sButtonSelector).click(function () {
        sc.createDialog();
        return false;
      });
    };

    SpecialChars.prototype.insertChar = function (character) {
      var wym = this._wym;
      var sc = this;

      var char_info = sc.CHARS[character];

      wym.insert(char_info[1]);
    };

    SpecialChars.prototype.createDialog = function () {
      var wym = this._wym;
      var sc = this;
      var options = sc._options;

      var features = sc._options.dialogFeatures;
      var wDialog = window.open('', 'dialog', features);
      var dialogType = "Insert Symbol";

      sBodyHtml = options.sBodyHtml;

      var h = WYMeditor.Helper;

      //construct the dialog
      var dialogHtml = options.dialogHtml;
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.BASE_PATH, wym._options.basePath);
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.DIRECTION, wym._options.direction);
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.CSS_PATH, wym._options.skinPath + WYMeditor.SKINS_DEFAULT_CSS);
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.WYM_PATH, wym._options.wymPath);
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.JQUERY_PATH, wym._options.jQueryPath);
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.DIALOG_BODY, sBodyHtml);
      dialogHtml = h.replaceAll(dialogHtml, WYMeditor.INDEX, wym._index);

      dialogHtml = wym.replaceStrings(dialogHtml);

      var doc = wDialog.document;
      doc.write(dialogHtml);
      doc.close();
    }

    SpecialChars.prototype.getCharMapHtml = function () {
      var cols = -1;
      var wym = this._wym;
      var sc = this;

      if (sc._dia_inner_html.length > 0) {
        return sc._dia_inner_html;
      }
      var html = '';
      html += '<div class="wym_special_chars">'
          + '<table border="0" cellspacing="1" cellpadding="0" '
          + 'width="'
          + (sc._options.tdWidth * sc._options.charsPerRow)
          + '" ><tr height="'
          + sc._options.tdHeight
          + '">',

      jQuery.each(sc.CHARS, function (index, item) {
        html += ''
            + '<td class="'
            + sc._options.charmapTdClass
            + '">'
            + '<span class="wym_add_special_char" id="charmap_'
            + index
            + '" title="'
            + item[2]
            + '">'
            + item[0]
            + '</span></td>';
        if ((cols + 1) % sc._options.charsPerRow == 0) {
          html += '</tr><tr height="' + sc._options.tdHeight + '">';
        }

        cols++;
      });

      html += '</tr></table></div>';

      sc._dia_inner_html = html;

      return html;
    }
    
    return my;
  
}(AUTHOR || {}));