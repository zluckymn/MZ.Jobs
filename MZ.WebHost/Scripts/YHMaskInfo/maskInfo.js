; (function ($) {
    $.fn.maskInfo = function (options) {
        return this.each(function () {
            var defaults={
			    loadType : "10",       //调用类型
                bgStyleList : "",    //背景样式
                fontStyleList : ""   //字体动画样式
            }
            var opts = $.extend(defaults, options);
            var $obj = $(this),w = $obj.width(),h=$obj.height();
            var htmlDom = "";
            var offset = 'top:'+Number(($(window).height()*0.5)-70)+'px;left:'+Number((0.5*w)-70)+'px;position:fixed;margin:5px;border-radius:5px;z-index:11000;';
            switch(Number(opts.loadType)){
                case 1:
                    htmlDom='<div class="progress-bar_box" id="caseVerte" style="'+ offset +'">';
                    for(var i=0;i<5;i++){
                        htmlDom+='<div id="case'+i+'"></div>';
                    }
                    htmlDom+='<div class="progress-bar_box" id="load"><p>loading ...</p></div></div>';
                    break;
                case 2:
                    htmlDom = '<div class="progress-bar_box" id="caseRouge" style="'+ offset +'"><div id="load"><p>loading ...</p></div><div id="progress-bar_top"></div><div id="progress-bar_left"></div><div id="progress-bar_right"></div></div>';
                    break;
                case 3:
                    htmlDom = '<div class="progress-bar_box" id="caseMarron" style="'+ offset +'"><div id="boule"></div><div id="load"><p>loading ...</p></div></div>';
                    break;
                case 4:
                    htmlDom = '<div class="progress-bar_box" id="caseViolette" style="'+ offset +'"><div id="cercle"><div id="cercleCache"></div></div>'+
                              '<div id="load"><p>loading</p></div><div id="point"></div></div>';
                    break;
                case 5:
                    htmlDom = '<div class="progress-bar_box" id="caseBlanche" style="'+ offset +'"><div id="rond"><div id="test"></div></div>'+
                              '<div id="load"><p>loading</p></div></div>';
                    break;
                case 6:
                    htmlDom = '<div class="progress-bar_box" id="casePourpre" style="'+ offset +'"><div id="load"><p>loading</p></div><div id="vague">';
                    for(var i=1;i<7;i++){
                        htmlDom+='<div id="vague'+i+'"></div>'
                    }
                    htmlDom+='</div></div>';
                    break;
                case 7:
                    htmlDom = '<div class="progress-bar_box" id="caseVerteClaire" style="'+ offset +'"><div id="transform">'+
                              '<div id="transform1"></div><div id="transform2"></div><div id="transform3"></div>'+
                              '</div><div id="load"><p>loading</p></div></div>';
                    break;
                case 8:
                    htmlDom = '<div class="progress-bar_box" id="caseGrise" style="'+ offset +'"><div id="progress"><div id="charge"></div></div>'+
                              '<div id="load"><p>loading</p></div></div>';
                    break;
                case 9:
                    htmlDom = '<div class="progress-bar_box" id="caseMarronFonce" style="'+ offset +'"><div id="spinner"></div></div>';
                    break;
                case 10:
                    htmlDom = '<div class="spinner progress-bar_box" style="'+ offset +'"><div class="spinner-container container1"><div class="circle1"></div><div class="circle2"></div><div class="circle3"></div><div class="circle4"></div></div>'+
                              '<div class="spinner-container container2"><div class="circle1"></div><div class="circle2"></div><div class="circle3"></div><div class="circle4"></div></div>'+
                              '<div class="spinner-container container3"><div class="circle1"></div><div class="circle2"></div><div class="circle3"></div><div class="circle4"></div></div></div>';
                    break;
                default:
                    console.info("暂无该类型");
                    return false;
            }
            if (window.applicationCache) {
                $obj.mask();
                $obj.append(htmlDom);
            } else {
                $obj.mask("加载中...");
            }
        });
    }
    $.fn.unmaskInfo = function () {
        return this.each(function () {
            $obj = $(this);
            $obj.unmask();
            if($obj.find(".progress-bar_box").length>0){
                $obj.find(".progress-bar_box").remove();
            }
        });
    }
})(jQuery);