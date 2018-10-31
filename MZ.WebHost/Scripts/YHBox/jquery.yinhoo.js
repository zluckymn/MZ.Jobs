// @#10001 修改了jqueryui dialog插件中源代码，使得dialog右上角关闭按钮可配置关闭弹框使用为close，或者destroy方法
// @#10000 修改了jqueryui dialog插件中源代码，使得dialog加载链接弹框过程中可以把弹框中代码删除

//返回顶部
$(document).delegate('.yh-j-goTop','click',function(){
    $('html,body').animate({scrollTop:0});
});
//删除按钮
$(document).delegate('.yh-j-close','click',function(){
    $(this).parent().remove();
});



//点击添加class siblings删除class
//close点击添加class siblings删除class
//仿ellipse段落生成“...”

/* ===========================================================
 * bootstrap-tooltip.js v2.3.2
 * http://getbootstrap.com/2.3.2/javascript.html#tooltips
 * Inspired by the original jQuery.tipsy by Jason Frame
 * ===========================================================
 * Copyright 2013 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ========================================================== */


!function ($) {

    "use strict"; // jshint ;_;


    /* TOOLTIP PUBLIC CLASS DEFINITION
     * =============================== */

    var Tooltip = function (element, options) {
        this.init('tooltip', element, options)
    }

    Tooltip.prototype = {

        constructor: Tooltip

        , init: function (type, element, options) {
            var eventIn
                , eventOut
                , triggers
                , trigger
                , i

            this.type = type
            this.$element = $(element)
            this.options = this.getOptions(options)
            this.enabled = true

            triggers = this.options.trigger.split(' ')

            for (i = triggers.length; i--;) {
                trigger = triggers[i]
                if (trigger == 'click') {
                    this.$element.on('click.' + this.type, this.options.selector, $.proxy(this.toggle, this))
                } else if (trigger != 'manual') {
                    eventIn = trigger == 'hover' ? 'mouseenter' : 'focus'
                    eventOut = trigger == 'hover' ? 'mouseleave' : 'blur'
                    this.$element.on(eventIn + '.' + this.type, this.options.selector, $.proxy(this.enter, this))
                    this.$element.on(eventOut + '.' + this.type, this.options.selector, $.proxy(this.leave, this))
                }
            }

            this.options.selector ?
                (this._options = $.extend({}, this.options, { trigger: 'manual', selector: '' })) :
                this.fixTitle()
        }

        , getOptions: function (options) {
            options = $.extend({}, $.fn[this.type].defaults, this.$element.data(), options)

            if (options.delay && typeof options.delay == 'number') {
                options.delay = {
                    show: options.delay
                    , hide: options.delay
                }
            }

            return options;
        }

        , enter: function (e) {
            var defaults = $.fn[this.type].defaults
                , options = {}
                , self

            this._options && $.each(this._options, function (key, value) {
                if (defaults[key] != value) options[key] = value
            });

            self = $(e.currentTarget)[this.type](options).data(this.type)

            if (!self.options.delay || !self.options.delay.show) return self.show()

            clearTimeout(this.timeout)
            self.hoverState = 'in'
            this.timeout = setTimeout(function() {
                if (self.hoverState == 'in') self.show()
            }, self.options.delay.show)
        }

        , leave: function (e) {

            var self = $(e.currentTarget)[this.type](this._options).data(this.type)

            if (this.timeout) clearTimeout(this.timeout)
            //if (!self.options.delay || !self.options.delay.hide) return self.hide()//源码隐藏

            self.hoverState = 'out'
            this.timeout = setTimeout(function() {
                if (self.hoverState == 'out') self.hide()
            }, self.options.delay.hide)

            //以下为添加的代码-start
            var that=this;
            self.$tip.mouseenter(function(){
                clearTimeout(that.timeout)
            }).mouseleave(function(){
                self.hide()
            })
            //以下为添加的代码-end
        }

        , show: function () {
            var $tip
                , pos
                , actualWidth
                , actualHeight
                , placement
                , tp
                , e = $.Event('show')

            if (this.hasContent() && this.enabled) {
                this.$element.trigger(e)
                if (e.isDefaultPrevented()) return
                $tip = this.tip()
                this.setContent()

                if (this.options.animation) {
                    $tip.addClass('fade')
                }

                placement = typeof this.options.placement == 'function' ?
                    this.options.placement.call(this, $tip[0], this.$element[0]) :
                    this.options.placement

                $tip
                    .detach()
                    .css({ top: 0, left: 0, display: 'block' })

                this.options.container ? $tip.appendTo(this.options.container) : $tip.insertAfter(this.$element)

                pos = this.getPosition()

                actualWidth = $tip[0].offsetWidth
                actualHeight = $tip[0].offsetHeight

                switch (placement) {
                    case 'bottom':
                        tp = {top: pos.top + pos.height, left: pos.left + pos.width / 2 - actualWidth / 2}
                        break
                    case 'top':
                        tp = {top: pos.top - actualHeight, left: pos.left + pos.width / 2 - actualWidth / 2}
                        break
                    case 'left':
                        tp = {top: pos.top + pos.height / 2 - actualHeight / 2, left: pos.left - actualWidth}
                        break
                    case 'right':
                        tp = {top: pos.top + pos.height / 2 - actualHeight / 2, left: pos.left + pos.width}
                        break
                }

                this.applyPlacement(tp, placement)
                this.$element.trigger('shown')
            }
        }

        , applyPlacement: function(offset, placement){
            var $tip = this.tip()
                , width = $tip[0].offsetWidth
                , height = $tip[0].offsetHeight
                , actualWidth
                , actualHeight
                , delta
                , replace

            $tip
                .offset(offset)
                .addClass(placement)
                .addClass('in')

            actualWidth = $tip[0].offsetWidth
            actualHeight = $tip[0].offsetHeight


            if (placement == 'top' && actualHeight != height) {
                offset.top = offset.top + height - actualHeight
                replace = true
            }

            if (placement == 'bottom' || placement == 'top') {
                delta = 0

                if (offset.left < 0){
                    delta = offset.left * -2
                    offset.left = 0
                    $tip.offset(offset)
                    actualWidth = $tip[0].offsetWidth
                    actualHeight = $tip[0].offsetHeight
                }

                this.replaceArrow(delta - width + actualWidth, actualWidth, 'left')
            } else {
                this.replaceArrow(actualHeight - height, actualHeight, 'top')
            }

            if (replace) $tip.offset(offset)
        }

        , replaceArrow: function(delta, dimension, position){
            this
                .arrow()
                .css(position, delta ? (50 * (1 - delta / dimension) + "%") : '')
        }

        , setContent: function () {
            var $tip = this.tip()
                , title = this.getTitle()

            $tip.find('.yh-tooltip-inner')[this.options.html ? 'html' : 'text'](title)
            $tip.removeClass('fade in top bottom left right')
        }

        , hide: function () {
            var that = this
                , $tip = this.tip()
                , e = $.Event('hide')

            this.$element.trigger(e)
            if (e.isDefaultPrevented()) return

            $tip.removeClass('in')

            function removeWithAnimation() {
                var timeout = setTimeout(function () {
                    $tip.off($.support.transition.end).detach()
                }, 500);

                $tip.one($.support.transition.end, function () {
                    clearTimeout(timeout)
                    $tip.detach()
                });
            }

            $.support.transition && this.$tip.hasClass('fade') ?
                removeWithAnimation() :
                $tip.detach();

            this.$element.trigger('hidden');
            return this;
        }

        , fixTitle: function () {
            var $e = this.$element
            if ($e.attr('title') || typeof($e.attr('data-original-title')) != 'string') {
                $e.attr('data-original-title', $e.attr('title') || '').attr('title', '')
            }
        }

        , hasContent: function () {
            return this.getTitle();
        }

        , getPosition: function () {
            var el = this.$element[0]
            return $.extend({}, (typeof el.getBoundingClientRect == 'function') ? el.getBoundingClientRect() : {
                width: el.offsetWidth
                , height: el.offsetHeight
            }, this.$element.offset());
        }

        , getTitle: function () {
            var title
                , $e = this.$element
                , o = this.options

            title = $e.attr('data-original-title')
                || (typeof o.title == 'function' ? o.title.call($e[0]) :  o.title);

            return title;
        }

        , tip: function () {
            return this.$tip = this.$tip || $(this.options.template)
        }

        , arrow: function(){
            return this.$arrow = this.$arrow || this.tip().find(".yh-tooltip-arrow")
        }

        , validate: function () {
            if (!this.$element[0].parentNode) {
                this.hide()
                this.$element = null
                this.options = null
            }
        }

        , enable: function () {
            this.enabled = true
        }

        , disable: function () {
            this.enabled = false
        }

        , toggleEnabled: function () {
            this.enabled = !this.enabled
        }

        , toggle: function (e) {
            /*
            * 此处注释的代码是是bootstrap源码
            * var self = e ? $(e.currentTarget)[this.type](this._options).data(this.type) : this
            * self.tip().hasClass('in') ? self.hide() : self.show();
            *
            * */


            var self = e ? $(e.currentTarget)[this.type](this._options).data(this.type) : this;

            if(self.tip().hasClass('in')){
                self.hide();
            }else{
                self.show();

                if(1){
                    var hidePopover=function(e){
                        if($(e.target).closest(self.$tip).length<1){
                            self.hide();
                            $(document).off('click',hidePopover)
                        }
                    }
                    setTimeout(function(){
                        $(document).on('click',hidePopover)
                    })
                }
            }
        }

        , destroy: function () {
            this.hide().$element.off('.' + this.type).removeData(this.type)
        }

    }


    /* TOOLTIP PLUGIN DEFINITION
     * ========================= */

    var old = $.fn.tooltip

    $.fn.tooltip = function ( option ) {
        return this.each(function () {
            var $this = $(this)
                , data = $this.data('tooltip')
                , options = typeof option == 'object' && option
            if (!data) $this.data('tooltip', (data = new Tooltip(this, options)))
            if (typeof option == 'string') data[option]()
        })
    }

    $.fn.tooltip.Constructor = Tooltip

    $.fn.tooltip.defaults = {
        animation: true
        , placement: 'top'
        , selector: false
        , template: '<div class="yh-tooltip"><div class="yh-tooltip-arrow"></div><div class="yh-tooltip-inner"></div></div>'
        , trigger: 'hover focus'
        , title: ''
        , delay: 0
        , html: false
        , container: false
    }


    /* TOOLTIP NO CONFLICT
     * =================== */

    $.fn.tooltip.noConflict = function () {
        $.fn.tooltip = old
        return this
    }

}(window.jQuery);
/* ===========================================================
 * bootstrap-popover.js v2.3.2
 * http://getbootstrap.com/2.3.2/javascript.html#popovers
 * ===========================================================
 * Copyright 2013 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * =========================================================== */


!function ($) {

    "use strict"; // jshint ;_;


    /* POPOVER PUBLIC CLASS DEFINITION
     * =============================== */

    var Popover = function (element, options) {
        this.init('popover', element, options)
    }


    /* NOTE: POPOVER EXTENDS BOOTSTRAP-TOOLTIP.js
     ========================================== */

    Popover.prototype = $.extend({}, $.fn.tooltip.Constructor.prototype, {

        constructor: Popover

        , setContent: function () {
            var $tip = this.tip()
                , title = this.getTitle()
                , content = this.getContent()
            if(title){
                $tip.find('.yh-popover-title')[this.options.html ? 'html' : 'text'](title)
            }else{
                $tip.find('.yh-popover-title').remove();
            }
            $tip.find('.yh-popover-content')[this.options.html ? 'html' : 'text'](content)

            $tip.removeClass('fade top bottom left right in')
        }

        , hasContent: function () {
            return this.getTitle() || this.getContent()
        }

        , getContent: function () {
            var content
                , $e = this.$element
                , o = this.options

            content = (typeof o.content == 'function' ? o.content.call($e[0]) :  o.content)
                || $e.attr('data-content')

            return content
        }

        , tip: function () {
            if (!this.$tip) {
                this.$tip = $(this.options.template)
            }
            return this.$tip
        }

        , destroy: function () {
            this.hide().$element.off('.' + this.type).removeData(this.type)
        }

    })


    /* POPOVER PLUGIN DEFINITION
     * ======================= */

    var old = $.fn.popover

    $.fn.popover = function (option) {
        return this.each(function () {
            var $this = $(this)
                , data = $this.data('popover')
                , options = typeof option == 'object' && option
            if (!data) $this.data('popover', (data = new Popover(this, options)))
            if (typeof option == 'string') data[option]()
        })
    }

    $.fn.popover.Constructor = Popover

    $.fn.popover.defaults = $.extend({} , $.fn.tooltip.defaults, {
        placement: 'right'
        , trigger: 'click'
        , content: ''
        , template: '<div class="yh-popover"><div class="arrow"></div><div class="yh-popover-content"></div></div>'
        , onPop : $.noop
    })


    /* POPOVER NO CONFLICT
     * =================== */

    $.fn.popover.noConflict = function () {
        $.fn.popover = old
        return this
    }

}(window.jQuery);
/* ============================================================
 * bootstrap-dropdown.js v2.0.0
 * http://twitter.github.com/bootstrap/javascript.html#dropdowns
 * ============================================================
 * Copyright 2012 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ============================================================ */


!function( $ ){

    "use strict"

    /* DROPDOWN CLASS DEFINITION
     * ========================= */

    var toggle = '.yh-j-dropdown-toggle'
        , Dropdown = function ( element ) {
            var $el = $(element).on('click.dropdown.data-api', this.toggle)
            $('html').on('click.dropdown.data-api', function () {
                $el.parent().removeClass('open')
            })
        }

    Dropdown.prototype = {

        constructor: Dropdown

        , toggle: function ( e ) {
            var $this = $(this)
                , selector = $this.attr('data-target')
                , $parent
                , isActive

            if (!selector) {
                selector = $this.attr('href')
                selector = selector && selector.replace(/.*(?=#[^\s]*$)/, '') //strip for ie7
            }

            $parent = $(selector)
            $parent.length || ($parent = $this.parent())

            isActive = $parent.hasClass('open')

            clearMenus()
            !isActive && $parent.toggleClass('open')

            return false
        }

    }

    function clearMenus() {
        $(toggle).parent().removeClass('open')
    }

    /* DROPDOWN PLUGIN DEFINITION
     * ========================== */

    $.fn.dropdown = function ( option ) {
        return this.each(function () {
            var $this = $(this)
                , data = $this.data('dropdown')
            if (!data) $this.data('dropdown', (data = new Dropdown(this)))
            if (typeof option == 'string') data[option].call($this)
        })
    }

    $.fn.dropdown.Constructor = Dropdown


    /* APPLY TO STANDARD DROPDOWN ELEMENTS
     * =================================== */

    $(function () {
        $('html').on('click.dropdown.data-api', clearMenus)
        $('body').on('click.dropdown.data-api', toggle, Dropdown.prototype.toggle)
    })

}( window.jQuery )


/* ========================================================
 * bootstrap-tab.js v2.0.0
 * http://twitter.github.com/bootstrap/javascript.html#tabs
 * ========================================================
 * Copyright 2012 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ======================================================== */


!function( $ ){

    "use strict"

    /* TAB CLASS DEFINITION
     * ==================== */

    var Tab = function ( element ) {
        this.element = $(element)
    }

    Tab.prototype = {

        constructor: Tab

        , show: function () {
            var $this = this.element
                , $ul = $this.closest('ul:not(.dropdown-menu)')
                , selector = $this.attr('data-target')
                , previous
                , $target

            if (!selector) {
                selector = $this.attr('href')
                selector = selector && selector.replace(/.*(?=#[^\s]*$)/, '') //strip for ie7
            }

            if ( $this.parent('li').hasClass('active') ) return

            previous = $ul.find('.active a').last()[0]

            $this.trigger({
                type: 'show'
                , relatedTarget: previous
            })

            $target = $(selector)

            this.activate($this.parent('li'), $ul)
            this.activate($target, $target.parent(), function () {
                $this.trigger({
                    type: 'shown'
                    , relatedTarget: previous
                })
            })
        }

        , activate: function ( element, container, callback) {
            var $active = container.find('> .active')
                , transition = callback
                    && $.support.transition
                    && $active.hasClass('fade')

            function next() {
                $active
                    .removeClass('active')
                    .find('> .dropdown-menu > .active')
                    .removeClass('active')

                element.addClass('active')

                if (transition) {
                    element[0].offsetWidth // reflow for transition
                    element.addClass('in')
                } else {
                    element.removeClass('fade')
                }

                if ( element.parent('.dropdown-menu') ) {
                    element.closest('li.dropdown').addClass('active')
                }

                callback && callback()
            }

            transition ?
                $active.one($.support.transition.end, next) :
                next()

            $active.removeClass('in')
        }
    }


    /* TAB PLUGIN DEFINITION
     * ===================== */

    $.fn.tab = function ( option ) {
        return this.each(function () {
            var $this = $(this)
                , data = $this.data('tab')
            if (!data) $this.data('tab', (data = new Tab(this)))
            if (typeof option == 'string') data[option]()
        })
    }

    $.fn.tab.Constructor = Tab


    /* TAB DATA-API
     * ============ */

    $(function () {
        $('body').on('click.tab.data-api', '[data-toggle="tab"], [data-toggle="pill"]', function (e) {
            e.preventDefault()
            $(this).tab('show')
        })
    })

}( window.jQuery )


;(function($){
    $.YH ={
        script:{
            jqueryui:{
                js:'/webstorm/Projects/js/jquery-ui-1.9.2.custom.min.js',
                    css:'/webstorm/Projects/js/jQuery/JQueryUi/css/jquery-ui-1.9.2.custom.css',
                    key:['jQuery.ui','object']
            },
            jcrop:{
                js:'/webstorm/Projects/js/jQuery/Jcrop/js/jquery.Jcrop.min.js',
                    css:'/webstorm/Projects/js/jQuery/Jcrop/css/jquery.Jcrop.min.css',
                    key:['jQuery.fn.Jcrop','function']
            },
            YHsuperTable:{js:'/webstorm/Projects/js/table/jquery.YH-superTables.js',key:['jQuery.fn.YHsuperTable','function']},
            YHsly:{js:'/webstorm/Projects/js/jQuery/sly/jquery.YH-sly.js',key:['jQuery.fn.YHsly','function']},
            cookie:{js:'/webstorm/Projects/js/jQuery/jquery.cookie.js',key:['jQuery.cookie','function']},
            colorpicker:{}
        },
        testFunction:function (lib,context,fn){
            if(typeof eval('('+$.YH.script[lib].key[0]+')') ===$.YH.script[lib].key[1]){
                fn.call(context);
            }else if($.type($.YH.script[lib].fn)==="undefined"){

                $.getScript($.YH.script[lib].js,function(){
                    //alert(typeof eval('('+lib+')'));
                    if($.YH.script[lib].css){
                        var css = document.createElement("link");
                        css.href = $.YH.script[lib].css;
                        css.type="text/css";
                        css.rel="stylesheet";
                        document.getElementsByTagName('head').item(0).appendChild(css);
                    }
                    //$("<link>").attr({ rel: "stylesheet",type: "text/css",href: $.YH.script[lib].css}).appendTo("head");
                    for(var i=0,len=$.YH.script[lib].fn.length;i<len;i++){
                        //alert(script[lib][i][1]);
                        $.YH.script[lib].fn[i][1].call($.YH.script[lib].fn[i][0]);
                    }
                });
                $.YH.script[lib].fn=[];
                $.YH.script[lib].fn.push([context,fn]);
            }else if($.type($.YH.script[lib].fn)==='array'){
                $.YH.script[lib].fn.push([context,fn]);
            }
        }
    }
    $.extend($.YH,{
//        box : function(arg){
//            var $box;
//            var options={
//                autoOpen: true,
//                modal: true,
//                width:600,
//                title:'弹框提示',
//                buttons:{
//                    '确定':{
//                        click:function () {
//                            if ($.type(arg.ok) === 'function') {
//                                if (false === arg.ok.call(this)) return;
//                            }
//                            if (options.destroy) {
//                                $(this).dialog("destroy");
//                            } else {
//                                $(this).dialog("close");
//                            }
//                        },
//                        text:'确定',
//                        'class':"ui-button-primary"
//
//                    },
//                    '取消':function () {
//                        if($.type(arg.cancel) ==='function'){
//                            if(false===arg.cancel.call(this)) return ;
//                        }
//                        if(options.destroy){
//                            $(this).dialog("destroy");
//                        }else{
//                            $(this).dialog("close");
//                        }
//                    }
//                },
//                // @#10001基于jqueryui扩展更多的参数，选择弹框关闭按钮的关闭类型close或destroy
//                destroy:true,
//                // @#10002基于jqueryui扩展更多的参数，弹框销毁前执行的回调函数
//                beforeDestroy: $.noop
//
//            };
//            $.extend(options,arg);
//            if(options.target.constructor===$){
//                $box=options.target.dialog(options);
//            }else if(options.target.indexOf('/')===-1 && $(options.target).length>0){
//                $box=$(options.target).dialog(options);
//            }else{
//                $box=$('<div>').dialog(options).load(options.target);
//            }
//            return $box;
//        },

//        isIE:function(){
//        },
//
//        isFirefox:function(){
//        },
//
//        isIE:function(){
//        },
//
//        isIE:function(){
//        },
//
//        isIE:function(){
//        },
//
//        isIE:function(){
//        }

    })
})(window.jQuery);

// 弹框基于jqueryui封装
// @#10001基于jqueryui扩展更多的参数，选择弹框关闭按钮的关闭类型close或destroy
// @#10002基于jqueryui扩展更多的参数，弹框销毁前执行的回调函数
;(function($,window,undefined){
    $.extend($.YH, {
        box: function (options) {
            var $box;
            var opts=$.extend({}, $.YH.box.defaults,options);
            if(!options.buttons){
                opts.buttons=$.extend(true,{}, $.YH.box.defaults.buttons);
            }
            if(typeof opts.buttonNames[0] === 'string'){
                opts.buttons['确定']['text']=opts.buttonNames[0];
            }else if($.type(opts.buttonNames[0]) === 'null'){
                delete opts.buttons['确定'];
            }
            if(typeof opts.buttonNames[1] === 'string'){
                opts.buttons['取消']['text']=opts.buttonNames[1];
            }else if($.type(opts.buttonNames[1]) === 'null'){
                delete opts.buttons['取消'];
            }
            if (opts.target.constructor === $) {
                $box = opts.target.dialog(opts);
            } else if (opts.target.indexOf('/') === -1 && $(opts.target).length > 0) {
                $box = $(opts.target).dialog(opts);
            } else {
                $box = $('<div class="yh-loading">').dialog(opts).load(opts.target, opts.data, function(){
                    $box.removeClass('yh-loading');
                    var alDiv = $box.parent();
                    var alHeih = alDiv.height();
                    var alTop = $(document).scrollTop() + $(window).height() / 2;
                    alDiv.css({ "top": alTop - alHeih / 2 });

                    opts.afterLoaded.apply($box[0],arguments);
                });
            }
            $box[0].opts=opts;

            if(opts.fixedBody){
                $box.mouseenter(function(){
                    $('body').addClass('oh')
                }).mouseleave(function(){
                    $('body').removeClass('oh')
                });
            }

            return $box;
        }
    });
    $.extend($.YH.box,{
        defaults: {
            target:'',
            title: '弹框提示',
            width: 600,
            autoOpen: true,
            modal: true,
            resizable:true,
            appendTo:'',
            closeOnEscape:true,
            closeText:'',
            dialogClass:'',
            draggable:true,
            height:'auto',
            show:false,
            hide:false,
            maxHeight:false,
            maxWidth:false,
            minHeight:150,
            minWidth:150,
            autoFocus:true,
            fixedBody:false,
            position:{ my: "center", at: "center", of: window },
            buttonNames:[],
            buttons: {
                '确定': {
                    click: function () {
                        if ($.type(this.opts.ok) === 'function') {
                            if (false === this.opts.ok.call(this)) return;
                        }
                        if (this.opts.destroy) {
                            $(this).dialog("destroy");
                        } else {
                            $(this).dialog("close");
                        }
                    },
                    text: '确定',
                    'class': "ui-button-primary"
                },
                '取消':{
                    click: function () {
                        if ($.type(this.opts.cancel) === 'function') {
                            if (false === this.opts.cancel.call(this)) return;
                        }
                        if (this.opts.destroy) {
                            $(this).dialog("destroy");
                        } else {
                            $(this).dialog("close");
                        }
                    },
                    text: '取消'
                }
            },
            // @#10001基于jqueryui扩展更多的参数，选择弹框关闭按钮的关闭类型close或destroy
            destroy: true,
            //以下传入参数为回调函数
            // @#10002基于jqueryui扩展更多的参数，弹框销毁前执行的回调函数
            ok: $.noop,
            beforeDestroy: $.noop,
            close:$.noop,
            create:$.noop,
            drag:$.noop,
            dragStart:$.noop,
            dragStop:$.noop,
            focus:$.noop,
            open:$.noop,
            resize:$.noop,
            resizeStart:$.noop,
            resizeStop:$.noop,
            afterLoaded: $.noop,
            data: null
        },
        options: {
            target: '(必填)指定box的弹框内容，可以传人css选择器、url路径、jquery对象。 ',
            title: '(jqUI默认"弹框提示")指定box的标题文字。 ',
            width: '(jqUI默认600)设置box的宽度（单位：像素）',
            autoOpen: '(jqUI默认true)当设置为 true 时， box 会在初始化时自动打开. 如果为 false box 将会继续隐藏直到调用open()方法 。',
            modal: '(jqUI默认true)如果设置为false，该box将没有遮罩层; ',
            resizable:'(jqUI默认true)如果设置为false， 那么box不允许调整大小。',
            appendTo:'(jqUI选填)box（和遮罩层，如果modal存在）应该被追加到哪个元素。',
            closeOnEscape:'(jqUI默认true)指定具有焦点的box，在用户按下退出（ESC）键时，是否应该关闭 。',
            closeText:'（jqUI不建议配置）指定关闭按钮的文本。 注意，目前主题是基于bootstrap，不建议配置该属性。',
            dialogClass:'(jqUI选填)在使用额外附加的主题时，指定box的类名称，这些样式添加到box上。',
            draggable:'(jqUI默认true)如果设置为false, box将不可以使用标题栏实现拖动。',
            height:'(jqUI默认“auto”)设置对话框的高度（单位：像素）',
            show:'(jqUI默认false)box打开（显示）时的动画效果。',
            hide:'(jqUI默认false)box关闭（隐藏）时的动画效果。',
            maxHeight:'(jqUI默认false)box可以调整的最大高度，以像素为单位。',
            maxWidth:'(jqUI默认false)box可以调整的最大宽度，以像素为单位。',
            minHeight:'(jqUI默认150)box可以调整的最小高度，以像素为单位。',
            minWidth:'(jqUI默认150)box可以调整的最小宽度，以像素为单位。',
            autoFocus:'(默认true)box中的表单元素是否自动触发focus。',
            fixedBody:'(默认false)配置在鼠标移到box时body是否有滚动条。',
            position:'(jqUI选填)指定box显示的位置。该box将会处理冲突  ，使得尽可能多的box尽可能地可见。',
            buttonNames:'（默认[]）配置默认按钮文字',
            buttons: '(jqUI默认确定和取消按钮)配置弹框底部按钮，this指向box元素; 如果你需要访问按钮， 可以利用事件对象的目标元素。',
            destroy: '（@#10001默认true）配置窗口关闭时是否将窗口代码从页面中销毁，默认为销毁，false为关闭时隐藏在页面底部。',
            //以下传入参数为回调函数
            ok: '（选填）当点击窗口默认“确定”按钮时触发。fn() ',
            beforeDestroy: '（@#10002选填）当box即将销毁时触发。fn(event,{ui：{}}) ',
            beforeClose :'（jqUI选填）当box即将关闭时触发。 如果取消，box将不会关闭。fn(event,{ui：{})',
            close:'（jqUI选填）当box关闭时触发。fn(event,{ui：{})',
            create:'（jqUI选填）在创建box时触发。fn(event,{ui：{})',
            drag:'（jqUI选填）在box正在被拖动时触发。fn(event,{ui：{position:obj,offset:obj}})',
            dragStart:'（jqUI选填）当用户开始拖动box时触发。fn(event,{ui：{position:obj,offset:obj}})',
            dragStop:'（jqUI选填）当box 停止拖动时触发。fn(event,{ui：{position:obj,offset:obj}})',
            focus:'（jqUI选填）当对话框获取焦点时触发此事件。fn(event,{ui：{})',
            open:'（jqUI选填）当对话框打开后，触发此事件。fn(event,{ui：{})',
            resize:'（jqUI选填）当对话框大小改变时，触发此事件。fn(event,{ui：{originalPosition:obj,position:obj,originalSize:obj,size:obj})',
            resizeStart:'（jqUI选填）当开始改变对话框大小时，触发此事件。fn(event,{ui：{originalPosition:obj,position:obj,originalSize:obj,size:obj})',
            resizeStop:'（jqUI选填）当对话框改变大小后，触发此事件。fn(event,{ui：{originalPosition:obj,position:obj,originalSize:obj,size:obj})',
            afterLoaded: '（选填）仅当target参数为路径时，窗口完成对应路径载入后触发此事件。fn()',
            data: '（选填）仅当target参数为路径时，data为配置参数，传递到后台'
        },
        info:'弹框',
        url:'/Content/html/Widgets/box/box.html'
    });
})(jQuery,window)

//浮动导航
;(function ($,window,undefined) {
    $.extend($.fn, {
        scrollnav: function (options) {
            var opts = $.extend({}, $.fn.scrollnav.defaults,options);
            var $scrollbody,$scrollContent,sTop;
            if(opts.scrollTarget===window){
                $scrollbody=$(window);
                $scrollContent=$('html,body');
                sTop=0;
            }else{
                $scrollbody=$(opts.scrollTarget);
                $scrollContent=$scrollbody;
                sTop=$scrollbody.offset().top;
            }
            this.each(function () {
                var $that=$(this);
                var prefix;
                if(opts.setPrefix && typeof opts.setPrefix ==='string'){
                    prefix='#'+opts.setPrefix;
                }else{
                    prefix='#'+($(this).attr('id') || 'yh-j-scrollnav');
                }
                var scrollFn=function () {
                    $that.find('[href^="'+prefix+'"]').each(function () {
                        var $o = $($(this).attr('href'));
                        if($o.length>0 && $o.is(':visible')){
                            var isIn=false;
                            if(opts.scrollTarget === window){
                                isIn=$o.offset().top+10 > $scrollbody.scrollTop()+sTop && $o.offset().top  - ($scrollbody.scrollTop()+sTop) < $scrollbody.height()-50;
                            }else{
                                isIn=$o.offset().top-sTop<$scrollbody.height() && $o.offset().top+10 > sTop;
                            }
                            if (isIn) {
                                $that.find('.'+opts.cls).removeClass(opts.cls);
                                if(opts.addClassToParents){
                                    $(this).closest(opts.addClassToParents).addClass(opts.cls);
                                }else{
                                    $(this).addClass(opts.cls);
                                }
                                return false;
                            }
                        }
                    });
                }
                $scrollbody.on('scroll',scrollFn)
                $that.on('click','[href^="'+prefix+'"]',function(event){

                    var $this=$(this);
                    var $o = $($this.attr('href'));
                    var scroll;
                    if($o.length>0 && $o.is(':visible')){

                        if(opts.scrollTarget===window){
                            scroll=$o.offset().top;
                        }else{
                            scroll=$o.offset().top-sTop+$scrollbody.scrollTop();
                        }

                        $scrollbody.off('scroll',scrollFn);
                        $scrollContent.animate({scrollTop: scroll + opts.offsetY},function(){
                            $that.find('.'+opts.cls).removeClass(opts.cls);
                            $this.addClass(opts.cls);
                            $scrollbody.on('scroll',scrollFn);
                        });
                    }else{
                        $that.find('.'+opts.cls).removeClass(opts.cls)
                        $this.addClass(opts.cls);
                    }

                    return false;

                })
            })
            return this;
        }
    })
    $.extend($.fn.scrollnav, {
        defaults: {
            scrollTarget: window,
            cls: 'select',
            addClassToParents: false,
            setPrefix:false,
            offsetY: 0
        },
        info: '浮动导航',
        options: {
            scrollTarget: '（默认window）滚动的目标元素，参数为jquery选择器',
            cls: '（默认“select”）设置浮动导航中选中的选项卡的class',
            addClassToParents: '（默认false）中选的选项卡的class加到当前标签还是祖先的标签中。参数为jquery选择器或者false，祖先的标签按传入的jquery选择器获',
            setPrefix:'（默认false）a中href属性的前缀设置。默认前缀与当前滚动导航的id前缀一致',
            offsetY: '(默认0)设置y轴偏移'

        },
        url: ''
    })
    $.YH.scrollnav=$.fn.scrollnav;
})(jQuery, window)

// 节点移动的弹窗方法
;(function($,window,undefined){
    $.YH= $.extend($.YH,{
        moveNode:function (options) {
            var opts=$.extend({},$.YH.moveNode.defaults,options)
            var ajax={};
            var moveToId;
            var lv;
            var moveTo = function (ajax) {
                if (moveToId == undefined) {
                    alert("请选择节点");
                    return false;
                }
                var $currentNode = $('#div' + moveToId);
                do {
                    if ($currentNode.attr('id').replace('div', '') == opts.moveId) {
                        alert("不能以自身节点或者自身子节点作为参数");
                        return false;
                    } else {
                        $currentNode = $currentNode.parent();
                    }
                } while ($currentNode.attr('objid'))

                if (!opts.moveOutOfRoot && lv == "1" && (ajax.moveType === "prev" || ajax.moveType === "next")) {
                    alert("不能移动到根节点上方或下方");
                    return false;
                }
                ajax.data[opts.targetIdKey]=moveToId;
                $.ajax({
                    url: ajax.url,
                    type: ajax.type,
                    data: ajax.data,
                    dataType: 'json',
                    error: function () {
                        opts.onError.apply($box[0],arguments);
                        return false;
                    },
                    success: function (data) {
                        if(false===opts.onSuccess.call($box[0],data,ajax.type)){
                            return false;
                        }
                    }
                });
            }
            var btnConfig= {
                '节点上方': function () {
                    if(false===opts.onMove2prev.call($box[0])){
                        return false;
                    }
                    ajax.moveType='prev';
                    if(opts.prevData){
                        ajax.data = opts.prevData || {};
                        ajax.type='post';
                        ajax.url=opts.prevUrl;
                    }else{
                        ajax.data = opts.prevData || {};
                        ajax.type='get';
                        ajax.url=opts.nextUrl;
                    }
                    moveTo(ajax);
                },
                '节点下方': function () {
                    if(false===opts.onMove2next.call($box[0])){
                        return false;
                    }
                    ajax.moveType='next';
                    if(opts.nextData){
                        ajax.data=opts.nextData || {};
                        ajax.type='post';
                        ajax.url=opts.nextUrl;
                    }else{
                        ajax.data=opts.nextData || {};
                        ajax.type='get';
                        ajax.url=opts.nextUrl;
                    }
                    moveTo(ajax);
                }
            }
            if(!opts.disableChildNode){
                btnConfig['子节点']=function () {
                    if(false===opts.onMove2children.call($box[0])){
                        return false;
                    }
                    ajax.moveType='child';
                    if(opts.childrenData){
                        ajax.data=opts.childrenData || {};
                        ajax.type='post';
                        ajax.url=opts.childrenUrl;
                    }else{
                        ajax.type='get';
                        ajax.data=opts.childrenData || {};
                        ajax.url=opts.childrenUrl;
                    }
                    moveTo(ajax);
                }
            }
            var $box = $.YH.box({
                target: $('<div><div class="p-j-treeContent  yh-box-j-alsoResize" style="height: '+( opts.height-140 )+'px"></div></div>'),
                title: opts.title,
                width: opts.width,
                height: opts.height,
                zIndex: opts.zIndex,
                modal: true,
                buttons:btnConfig,
                create: function () {
                    $(this).find(".p-j-treeContent").SelectTree({
                        startXml: opts.treeXML,
                        defaultShowItemLv: 3,
                        _onClick: function (id, name, obj, node) {
                            moveToId = id;
                            lv=obj.attr('lv');
                            if(!opts.ousideRoot && lv==='1'){
                                $(this).YH_moveNode('disableChildNode');
                            }
                            return opts.onTreeClick.apply($box[0],arguments);
                        },
                        _afterPrint:function($tree){
                            $tree.find('.tree').addClass('yh-box-j-alsoResize')
                        }
                    });
                }
            });
        }
    });

    $.fn.extend({
        YH_moveNode:function(){
            this.each(function(){
            })
            return this;
        }
    })
    $.extend($.YH.moveNode,{
        defaults: {
            moveId:undefined,
            treeXML:'',
            prevUrl:'',
            childrenUrl:'',
            nextUrl:'',
            title:'移动节点',
            width:300,
            height:450,
            disableChildNode:false,
            targetIdKey:'moveToId',
            moveOutOfRoot:false,
            prevData:'',
            childrenData:'',
            nextData:'',
            ousideRoot:false,
            onMove2prev: $.noop,
            onMove2children: $.noop,
            onMove2next: $.noop,
            onSuccess: $.noop,
            onError: $.noop,
            onTreeClick: $.noop,
            zIndex: 1000
        },
        options: {
            moveId:'（必填）移动节点的id',
            treeXML:'（必填）载入目录树的xml地址',
            prevUrl:'（必填）节点上方提交对应的url地址',
            childrenUrl:'（必填）节点下方提交对应的url地址',
            nextUrl:'（必填）子节点提交对应的url地址',
            title:'（默认值为“移动节点”）弹框标题',
            width:'（默认值为“300”）弹框宽度',
            height:'（默认值为“450”）弹框高度',
            disableChildNode:'（默认值为“false”）屏蔽移动到子节点',
            targetIdKey:'（默认值为“moveToId”）选中目录树节点id对应的字段名',
            moveOutOfRoot:'（默认值为“false”）是否可将节点移动到根节点的上方或者下方',
            prevData:'（选填）节点上方提交对应post的json数据，此参数填写后提交参数方法为post',
            childrenData:'（选填）子节点提交对应post的json数据，此参数填写后提交参数方法为post',
            nextData:'（选填）节点下方提交对应post的json数据，此参数填写后提交参数方法为post',
            ousideRoot:'（默认值为“false”）设置节点是否可移动到目录树根目录以外',
            onMove2prev: '（选填）移动到节点上方的回调函数fn()',
            onMove2children: '（选填）移动到子节点的回调函数fn()',
            onMove2next: '（选填）移动到节点下方的回调函数fn()',
            onSuccess: '（选填）ajax提交成功的回调函数fn(data，type)，data为ajax后台传回的参数，type为事件类型为prev或child',
            onError: '（选填）ajax提交失败的回调函数fn()',
            onTreeClick: '（选填）目录树点击的回调函数fn(id, name, obj, node)',
            zIndex: '（默认1000）设置弹框的z-index层级'
        },
        info:'基于目录树结构的节点移动',
        url:'演示地址'
    })
})(jQuery,window);

;(function($,window,undefined){
    $.YH= $.extend($.YH,{
        box_prompt:function (options) {
            var opts=$.extend({},$.YH.box.defaults,$.YH.box_prompt.defaults,options);

            var key = opts.key ? opts.key+'：' : '';
            var inp=opts.textarea ?
                (typeof opts.textarea==="string" ? '<textarea class="yh-j-boxPrompt-textarea" style="vertical-align: top; '+opts.textarea+' ">'+opts.value+'</textarea>'
                    :'<textarea style="vertical-align: top;" class="w240 h100">'+opts.value+'</textarea>' )
                        : '<input class="ml10 yh-j-boxPrompt-input" type="text" value="'+opts.value+'">';

            opts.target=$('<div class="mt10 mb10">'+key+' '+inp+'</div>');

            opts.buttons={
                '确定': {
                    click: function () {
                        var val=$(this).find('textarea,input').val();
                        if ($.type(this.opts.ok) === 'function') {
                            if (false === this.opts.ok.call(this,val)) return;
                        }
                        if (this.opts.destroy) {
                            $(this).dialog("destroy");
                        } else {
                            $(this).dialog("close");
                        }
                    },
                    text: '确定',
                    'class': "ui-button-primary"

                },
                '取消': function () {
                    if ($.type(this.opts.cancel) === 'function') {
                        if (false === this.opts.cancel.call(this)) return;
                    }
                    if (this.opts.destroy) {
                        $(this).dialog("destroy");
                    } else {
                        $(this).dialog("close");
                    }
                }
            }
            var $box = $.YH.box(opts);
        }
    });
    $.YH.box_prompt.defaults=$.extend({},$.YH.box.defaults,{
        title:'编辑',
        width:350,
        key:'',
        value:'',
        textarea:false
    });
    $.YH.box_prompt.options=$.extend({},$.YH.box.options,{
        target:'(不填)',
        title: '(jqUI默认"弹框提示")指定box的标题文字。 ',
        width: '(jqUI默认350)设置box的宽度（单位：像素）',
        key:'(选填)文本框字段设置',
        value:'(选填)文本框默认值',
        textarea:'(默认false)是否将文本输入框以taxtarea展示，可传入布尔值或字符串，字符串应用为css样式'
    });
    $.YH.box_prompt.info='简单文本输入';
    $.YH.box_prompt.url='';
})(jQuery,window);


/*
 * 固定表头
 * 实例方法YH_fixThead(opts)调用的是工具方法YH_fixHead(elem,opts)，通过$.YH.YH_fixThead.defaults暴露参数
 * 基于livequery直接通过class为‘yh-j-fixThead’调用
 * */
  ;(function ($, window, undefined) {
    $.extend($.YH,{
        YH_fixThead:function(elem,opts){
            opts= $.extend({},$.YH.YH_fixThead.defaults,opts);
            var $this=$(elem),
                $fixedHead;
            if(!$this.data('YH_fixThead')){
                $this.data('YH_fixThead',true);
            }else{
                return;
            }
            if(!$this.children('thead').length){
                $.error('固定表格表头必须包含thead标签');
                return;
            }
            $(window).on('scroll resize',function(e){
                if(!$this.get(0).parentNode){
                    $fixedHead && $fixedHead.remove();
                    $(window).off('scroll resize',arguments.callee);
                    $this=null;
                    $fixedHead=null;
                    return;
                }
                if($this.is(':hidden')){
                    $fixedHead && $fixedHead.hide();
                    return;
                }else{
                    $fixedHead && $fixedHead.show();
                }
                if(
                    $this.offset().top < $(window).scrollTop()
                    && $this.offset().top+$this.height() > $(window).scrollTop()
                    && (!$fixedHead || e.type === 'resize')
                ){
                    var $thead=$this.children('thead')
                        ,tTop=$thead.offset().top
                        ,tLeft=$thead.offset().left
                        ,$html=$('<table class="'+opts.tableClass+'" style="position:fixed; top:'+opts.top+'px; left:'+tLeft+'px"><thead class="p-tableborder-title"></thead></table>')
                        ,zIndex=0;
                    $thead.find('td:visible,th:visible').each(function(){
                        var vAlign=$(this).css('vertical-align')
                            ,tAlign=$(this).css('text-align');
                        var $td=$(this);
                        var html = '<' + this.nodeName + ' class="' + ($td.attr('class') || '') + ' '+opts.cellClass+'" style="' +
                        'border:1px solid #e0e0e0;' +
                        'position:absolute;' +
                        'padding:0;' +
                        'vertical-align:middle;' +
                        'background:' + opts.background + ';'+
                        'z-index:' + (zIndex++) + ';' +
                        'top:' + ($td.offset().top - tTop) + 'px;' +
                        'left:' + ($td.offset().left - tLeft) + 'px;' +
                        'width:' + ($td.outerWidth() - 1) + 'px;' +
                        'height:' + ($td.outerHeight() - 1) + 'px;' +
                        '"><div style="display: table-cell;' +
                        'height: ' + ($td.outerHeight()-parseInt($td.css('padding-top'))-parseInt($td.css('padding-bottom'))) + 'px;' +
                        'vertical-align:'+vAlign+';' +
                        'padding-top:'+$td.css('padding-top')+';' +
                        'padding-left:'+$td.css('padding-left')+';' +
                        'padding-bottom:'+$td.css('padding-bottom')+';' +
                        'padding-right:'+$td.css('padding-right')+';'+
                        'text-align:'+tAlign+';' +
                        'width:' + ($td.outerWidth() -parseInt($td.css('padding-left'))-parseInt($td.css('padding-right'))- 2) + 'px;"></div></' + this.nodeName + '>';

                        //$html.append($(html).children().append($td.clone(true).attr("colspan",1)).end());
                        $html.find('thead').append($(html).children().append($td.html()).end());
                    });
                    $fixedHead && $fixedHead.remove();
                    $fixedHead=$html.insertAfter($this);
                }else if(($this.offset().top>$(window).scrollTop() || $this.offset().top+$this.height() < $(window).scrollTop()) && $fixedHead  ){
                    $fixedHead && $fixedHead.remove();
                    $fixedHead=null;
                }
            });
        }
    });
    $.extend($.YH.YH_fixThead,{
        info:'固定表头，其中表格必须包含thead标签，实例方法$.fn.YH_fixThead(opts)调用的是工具方法$.YH.fixThead(elem,opts)，通过class为‘yh-j-fixThead’调用',
        url:'',
        options:{
            top:'(默认‘0’)距离顶部的高度',
            tableClass:'(默认‘’)指定固定头部class',
            cellClass:'(默认‘’)指定固定头部内单元格class',
            background:'(默认‘#f3f3f3’)指定固定头部内单元格背景'
        },
        defaults:{
            top:0,
            tableClass:'',
            cellClass:'',
            background:'#f3f3f3'
        }
    });
    $.fn.extend({
        YH_fixThead:function(options){
            this.each(function(){
                $.YH.YH_fixThead(this,options);
            });
            return this;
        }
    });
})(jQuery,window)

    /*
     * 给对应节点添加参考坐标
     * 实例方法YH_YH_coord(opts)调用的是工具方法YH_coord(elem,options)，通过$.YH.YH_coord.defaults暴露参数
     * 基于livequery直接通过class为‘yh-j-coord’调用
     * */
;(function($,window,undefined){
    $.extend($.YH,{
        YH_coord:function(elem,options){
            var $elem=$(elem)
                ,$xAxes=null
                ,$yAxes=null
                ,opts= $.extend({}, $.YH.YH_coord.defaults,options);
            $elem.mouseenter(function(ev){
                var $this=$(this)
                    ,top=$this.offset().top
                    ,left=$this.offset().left
                    ,x=ev.pageX-left
                    ,y=ev.pageY-top;
                $xAxes=$('<div style="'+opts.xAxesStyle+' left:0; position: absolute"></div>').css({width:$this.width(),top:y});
                $yAxes=$('<div style="'+opts.yAxesStyle+' top:0; position: absolute"></div>').css({height:$this.height(),left:x});
                if($this.css('position')==='static'){
                    $($this).css('position','relative');
                }
                $this.append($xAxes,$yAxes).mousemove(function(e){
                    $xAxes.css('top', e.pageY-top-opts.xOffset);
                    $yAxes.css('left', e.pageX-left-opts.yOffset);
                });
            }).mouseleave(function(){
                $xAxes && $xAxes.add($yAxes).remove();
            });
            return $elem;
        }
    });
    $.extend($.fn,{
        YH_coord:function(options){
            this.each(function(){
                $.YH.YH_coord(this,options);
            });
            return this;
        }
    });
    $.extend($.YH.YH_coord,{
        info:'给对应节点添加参考坐标',
        url:'',
        defaults:{
            xAxesStyle:'height:1px;background: #666;opacity: 0.75; filter: alpha(opacity=65); z-index:100;',
            yAxesStyle:'width:1px;background: #666;opacity: 0.75; filter: alpha(opacity=65); z-index:100;',
            xOffset:1,
            yOffset:1
        },
        options:{
            xAxesStyle:'',
            yAxesStyle:'',
            xOffset:'',
            yOffset:''
        }
    });
})(jQuery,window);

/*
 * */
;(function($,window,undefined){
    $.extend($.YH,{
        exportPDF:function(opts){
            var opts= $.extend({},$.YH.exportPDF.defaults,opts)
                ,$form=$('<form id="exportPDF" action="'+opts.action+'" method="post">' +
                '<input type="text" name="cssLoad" value="'+opts.cssUrl+'"/>' +
                '<input type="text" name="pdfName" value="'+opts.pdfName+'"/>' +
                '<input type="text" name="pdfParams" value="'+opts.pdfParams+'"/>' +
                '<textarea name="html"></textarea>' +
                '</form>');
            $('body').append($form);
            $form.children('textarea').val(escape(opts.html)).end().submit().remove();
        }
    });
    $.extend($.YH.exportPDF,{
        info:'传入HTML和CSS将代码导出为pdf',
        url:'',
        defaults:{
            action:'/Home/CommonHtmlToPDF',
            cssUrl:'',
            pdfName:'未命名',
            html:'',
            pdfParams:''
        },
        options:{
            action:'（默认“/Home/CommonHtmlToPDF”）后端post的url接口',
            cssUrl:'（默认“”）html代码对应的css文件链接',
            pdfName:'（默认“未命名”）配置导出的pdf文件名',
            html:'（必填）传入需带出的html代码',
            pdfParams:'（默认“”）pdf配置参数'
        }
    });
})(jQuery,window);


/*
 * */
;(function($,window,undefined){
    var showIndex = function(index, $elem){
        $elem.find('ul:first li').eq(index).fadeIn().siblings().fadeOut()
        $elem.find('ul:eq(1) li').eq(index).css({'background': '#a42c2d','color':'#fff','border-color':'#a42c2d'}).siblings().css({'background': '#ccc', 'color':'#333','border':'1px solid #dedede'})
    }
    $.extend($.YH,{
        picSlide:function(elem, opts){
            opts = $.extend({}, $.YH.picSlide.defaults, opts);
            //opts.height = ''
                var $elem = $(elem),
                    len = $elem.find('li').length,
                    $btns = $('<ul style="position: absolute; right: 10px; bottom: 10px"></ul>'),
                    curIndex = len - 1,
                    buttonsHtml = "";
            $elem.css({
                height: opts.height,
                width: opts.width,
                position: 'relative'
            }).children('ul').width(opts.width).children().css({position: 'absolute', top:0, left:0 });
            for(var i = 0; i<len; i++){
                buttonsHtml += '<li style="float:right; height: 18px; width: 18px; background: #a42c2d none repeat scroll 0 0; text-align: center; cursor:pointer; margin-left: 5px">'+ (len - i) +'</li>'
            }
            $elem.append($btns.append(buttonsHtml));
            showIndex(curIndex, $elem);
            $btns.on('click','li',function(){
                showIndex($(this).index(), $elem);
            }).on('mouseenter','li',function(){
                $(this).fadeTo(500,0.8);
            }).on('mouseleave','li',function(){
                $(this).fadeTo(500,1);
            });
            setInterval(function(){
                curIndex = ++curIndex == len ? 0 : curIndex;
                showIndex(curIndex, $elem);
            },opts.interval);
        }
    });
    $.extend($.fn,{
        YH_picSlide:function(options){
            this.each(function(){
                $.YH.picSlide(this,options);
            });
            return this;
        }
    })
    $.extend($.YH.picSlide,{
        info:'图片轮播代码',
        url:'',
        defaults:{
            height: '',
            width: '',
            interval: 3000
        },
        options:{
            height: '（必填）配置图片轮播的高度',
            width: '（必填）配置图片轮播的宽度',
            interval: '（必填）配置图片轮播的时间间隔'
        }
    });
})(jQuery,window);

$(function () {
    //开启getscript缓存
    $.ajaxSetup({ cache: true });

    //优化通过组件动态调用对应js文件？？？？？？？？？？？？？

    $('.yh-j-fixThead').livequery(function () {
        var options = {}
            , $this = $(this);
        $this.attr('data-fixThead') && $.extend(options, eval('(' + $this.attr('data-fixThead') + ')'));
        $this.YH_fixThead(options);
    });

    $('.yh-j-coord').livequery(function () {
        var options = {}
            , $this = $(this);
        $this.attr('data-fixThead') && $.extend(options, eval('(' + $this.attr('data-fixThead') + ')'));
        $this.YH_coord(options);
    });

    $('.yh-j-datepicker').livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {
            var options = {
                changeMonth: true,
                changeYear: true,
                dateFormat: "yy-mm-dd",
                numberOfMonths: 1
            }
            $(this).attr('data-datepicker') && $.extend(options, eval('(' + $(this).attr('data-datepicker') + ')'));
            options.onClose = function (selectedDate) {

                options.from && $(options.from).datepicker("option", "maxDate", selectedDate);
                options.to && $(options.to).datepicker("option", "minDate", selectedDate);
            }
            //alert(options.toSource());
            $(this).datepicker(options);
        });
    });

    $('[data-tip]').livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {
            var str = $(this).attr('data-tip');
            var options = {};
            if (str !== undefined) {
                try {
                    options = eval('(' + str + ')');
                } catch (e) {
                    options = { title: str }
                }
            }
            var opts = $.extend({}, {
                title: $(this).attr('title'),
                placement: 'top',
                trigger: $(this).is('input') ? 'focus' : 'hover'
            }, options);
            $(this).tooltip(opts);
        });
    });

    $(document).delegate('.yh-j-dialog', 'click', function () {
        $.YH.testFunction('jqueryui', this, function () {

            var options = eval('(' + $(this).attr('data-dialog') + ')');
            // Dialog
            if (options.url) {
                $('<div>').load(options.url, function () {
                    openDialog(this);
                });
            } else {
                openDialog(options.target);
            }
            //
            function openDialog(obj) {
                $(obj).dialog({
                    autoOpen: true,
                    modal: true,
                    width: options.width || 600,
                    title: options.title || '弹框提示',
                    buttons: {
                        "Ok": function () {
                            $(this).dialog("close");
                        },
                        "Cancel": function () {
                            $(this).dialog("close");
                        }
                    }
                });
            }
        });
        return false;
    });


    $('.yh-j-validate').livequery(function () {
        $(this).validate();
    });

    $('.yh-j-buttonset').livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {
            $(this).buttonset();
        });
    });

    $('.yh-j-slider').livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {

            var arg = $(this).attr('data-slider') ? eval('(' + $(this).attr('data-slider') + ')') : {};
            $(this).attr('hidden', 'hidden').wrap('<div></div>');
            var options = {
                orientation: "horizontal",
                range: 'min',
                min: 0,
                max: 100,
                value: 60
            }
            $.extend(options, arg);
            options.slide = function (event, ui) {
                var val = ui.values ? ui.values[0] + " - " + ui.values[1] : ui.value;
                $(this).find('input').val(val);
                typeof arg.slide === 'function' && arg.slide(event, ui);
            }
            //$(this).wrap('<div></div>')
            $(this).parent('div').height(arg.height).slider(options);
        });
    });


    $('.yh-j-menu').livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {
            $(this).menu();
        });
    });
    $('.yh-j-spinner').livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {
            $(this).spinner();
        });
    });

    $('[yh-popover]').livequery(function () {
        var $this = $(this)
            , options = $this.attr('yh-popover').indexOf(':') > 0 ? eval('(' + $this.attr('yh-popover') + ')') : { content: $this.attr('yh-popover') };

        options = $.extend({}, {
            animation: true,
            html: false,
            placement: 'top',
            selector: false,
            trigger: 'click',
            title: '',
            content: '',
            delay: 0,
            container: 'body'
        }, options);
        $this.popover(options)
        if (options.trigger === 'click') {
            $this.click(function (e) {
                e.preventDefault()
            });
        }
    });
    $('[yh-tooltip]').livequery(function () {
        var $this = $(this)
            , options = $this.attr('yh-tooltip').indexOf(':') > 0 ? eval('(' + $this.attr('yh-tooltip') + ')') : { title: $this.attr('yh-tooltip') };

        options = $.extend({}, {
            animation: true,
            html: false,
            placement: 'top',
            selector: false,
            trigger: 'hover',
            title: '',
            delay: 0,
            container: 'body'
        }, options);
        $this.tooltip(options);
    });

    $('.yh-j-tooltip').livequery(function () {
        var $this = $(this);
        $this.tooltip({ title: $this.attr('title'), placement: 'top' });
    });

    $(".yh-j-autocomplete").livequery(function () {
        $.YH.testFunction('jqueryui', this, function () {
            var _arg = $(this).attr('data-autocomplete');
            var _availableData = _arg.search(',') > 0 ? _arg.split(',') : eval('(' + _arg + ')');
            $(this).autocomplete({
                source: _availableData
            });
        });
    });
    $(".yh-j-superTable").livequery(function () {
        $.YH.testFunction('YHsuperTable', this, function () {
            $(this).YHsuperTable({
                headerRows: 1,
                fixedCols: 2,
                colLength: 8,
                colAutoWidth: 200,
                colWidth: [100, 150, -1, 300],
                onStart: function () {
                    this.start = new Date();
                },
                onFinish: function () {
                    //alert("Finished... " + ((new Date()) - this.start) + "ms.");
                }
            });
        });
    });

    $(".yh-j-sly").livequery(function () {
        $.YH.testFunction('YHsly', this, function () {
            $(this).YHsly({
                maxHeight: '200',
                scrollBar: 'left',
                scrollBy: 150,
                startAt: 0
            });
        });
    });

    $('.yh-j-tab').livequery(function () {
        var $tab = $(this);
        var that = this;
        var initUrl;
        $tab.find('.yh-j-tab-head').each(function () {
            if ($(this).closest('.yh-j-tab')[0] == that) {
                if ($(this).closest('.yh-j-tab')[0] === that && $(this).hasClass('active') && $(this).attr('data-tab')) {
                    initUrl = $(this).attr('data-tab');
                    $tab.find('.yh-j-tab-body').each(function (index) {
                        if ($(this).closest('.yh-j-tab')[0] == that) {
                            $(this).load(initUrl);
                            return false;
                        }
                    });
                    return false;
                }
            }
        });
        $tab.delegate('.yh-j-tab-head', 'click', function () {
            var that = this,
                index = 0,
                i = 0,
                loadUrl,
                $tabHeads = $tab.find('.yh-j-tab-head').filter(function () {
                    if ($(this).closest('.yh-j-tab')[0] == $tab[0]) {

                        if (this === that) {
                            index = i;
                            $(this).addClass('active');
                            loadUrl = $(this).attr('data-tab');
                        } else {
                            $(this).removeClass('active');
                        }
                        i++;
                        return true;
                    } else {
                        return false;
                    }
                }),
                $tabBodys = $tab.find('.yh-j-tab-body').filter(function (index) {
                    return $(this).closest('.yh-j-tab')[0] == $tab[0];
                });
            if (loadUrl) {
                $tabBodys.eq(0).load(loadUrl);
            } else {
                $tabBodys.eq(index).addClass('active').siblings().removeClass('active');
            }
            return false;
        });
    });

    $('.yh-j-ellipsis').livequery(function () {
        var $this = $(this);
        var str = $.trim($this.html());
        var options = {
            length: 20
        }
        var _arg = $(this).attr('data-ellipsis') ? eval('(' + $(this).attr('data-ellipsis') + ')') : {};
        $.extend(options, _arg);
        if (str.length > options.length) {
            $(this).attr('ellipsis-text', str).html($('<span>' + str + '</span>').text().slice(0, options.length) + '<i class="ellipsis">...</i>').mouseenter(function () {
                $('<div style="position:absolute;z-index:1000;top:' + $this.position().top + 'px;left:' + $this.position().left + 'px;background:#fff;border:1px solid #eee;padding:5px; width:' + ($this.outerWidth() - 10) + 'px;">' + $(this).attr('ellipsis-text') + '</div>')
                    .appendTo('body')
                    .mouseleave(function () {
                        $(this).remove();
                    });
            });
        }
    });


    $('.yh-j-jcrop').livequery(function () {
        $.YH.testFunction('jcrop', this, function () {
            $(this).removeClass('yh-j-jcrop').Jcrop();
        });
    });
//    if (navigator.appName == "Microsoft Internet Explorer" && navigator.appVersion.match(/9./i) == 9) {
//        $(".p-tableborder").livequery(function () {
//            $(".p-tableborder").replaceTable();
//        });
//    }

    /*	$('.yh-j-ellipsis').each(function(){
    var $this=$(this);
    var str=$.trim($this.html());
    var options={
    length:40
    }
    if(str.length>options.length){
    $(this).attr('ellipsis-text',str).html(str.slice(0,options.length)+'<i class="ellipsis">...</i>').mouseenter(function(){
    $('<div style="position:absolute;z-index:1000;top:'+$this.offset().top+'px;left:'+$this.offset().left+'px;background:#fff;border:1px solid #eee;padding:5px; width:'+($this.outerWidth()-10)+'px;">'+$(this).attr('ellipsis-text')+'</div>')
    .appendTo('body')
    .mouseleave(function(){
    $(this).remove();
    });
    });
    }

    })*/


    //点击添加/删除class
    $(document).delegate('.yh-j-toggle', 'click', function (e) {
        var options = { 'This': 'this', 'cls': "select", target: '', before: '', after: '', rtn : 'true' };
        options['cls'] = $(this).attr('toggleClass') || 'select';
        options.target = $(this).attr('toggleTarget') || '';
        options.before = eval($(this).attr('toggleBefore'));
        options.after = eval($(this).attr('toggleAfter'));
        options['rtn'] = $(this).attr('toggleReturn');

        $.extend(options, eval("({" + $(this).attr('data-toggle') + "})"));

        options.rtn = typeof options.rtn == 'string' ? !(/^0$|^false$|^null$|^\ $|^$/).test(options.rtn) : options.rtn;

        if (typeof options.before == 'function') options.before(options.This, e);

        if (options.target === 'parent') {
            $(this).parent().toggleClass(options.cls);
        } else if (options.target) {
            $(options.target).toggleClass(options.cls);
        } else {
            $(this).toggleClass(options.cls);
        }
        if (typeof options.after == 'function') options.after(options.This, e);

        if (!options.rtn) {
            return false;
        }
    });


    //    //切换ppt演示的css
    //    $.YH.testFunction('cookie',this,function(){
    //        if ($.cookie("pptCSS")) {
    //            $('.yh-j-pptCSS').addClass('active');
    //            var css = document.createElement("link");
    //            css.href =$.cookie("pptCSS");
    //            css.id = 'pptCSS';
    //            css.type="text/css";
    //            css.rel="stylesheet";
    //            document.getElementsByTagName('head').item(0).appendChild(css);
    //        }
    //        $('.yh-j-pptCSS').click(function(){
    //            if($.cookie("pptCSS")){
    //                $.cookie("pptCSS",null,{path: '/'});
    //                $('#pptCSS').remove();
    //                $(this).removeClass('active');
    //            }else{
    //                $.cookie("pptCSS",$(this).attr('href'),{path: '/'});
    //
    //                var css = document.createElement("link");
    //                css.href =$(this).attr('href');
    //                css.id = 'pptCSS';
    //                css.type="text/css";
    //                css.rel="stylesheet";
    //                document.getElementsByTagName('head').item(0).appendChild(css);
    //
    //                $(this).addClass('active');
    //            }
    //            return false;
    //        })
    //    })

});

/*
处理IE9 表格因td tr之间空格造成的表格错位
*/
; (function ($, window, undefined) {
    $.fn.replaceTable = function () {
        return this.each(function(){
            var $obj = $(this);
            if (navigator.appName == "Microsoft Internet Explorer" && navigator.appVersion.match(/9./i) == 9) {
                var content = $obj.html().replace(/td>\s+<td/g, "td><td").replace(/tr>\s+<td/g,"tr><td").replace(/td>\s+<tr/g,"td><tr").replace(/tr>\s+<tr/g,"tr><tr");  //IE9兼容 bug
                $obj.html(content);
            }
        });
    }
})(jQuery, window);


;(function ($, window, undefined) {
    $.YH = $.YH || {};
    $.YH.suffixClass = {
        "arrExcelFileExt":
            [ ],
        "arrPdfFileExt":
            [".pdf", ".doc",  ".ppt", ".docx",  ".pptx", ".txt", ".rtf" ,".xls", ".xlsx"],
        "arrImageFileExt":
            [".jpg", ".gif", ".png", ".bmp", ".jpeg", ".tif", ".tiff"],
        "arrDwgFileExt":
            [".dwg", ".dxf", ".dwf"],
        "arrVedioFileExt":
            [".mp4", ".flv"]
    };
    $.extend($.YH, {
        readOnline: function (options,obj) {
            var guid = options.guid, name = options.name, id = options.id,
                type = options.type, ver = options.ver, ext = options.ext, fileUrl = options.fileUrl;
            options = $.extend({}, $.YH.readOnline.defaults, options);

            if (guid == "") {
                //$.tmsg("m_jfw", "文件尚未上传完成，请稍后阅读或刷新页面！", { infotype: 2, time_out: 1000 });
                layer.msg("文件尚未上传完成，请稍后阅读或刷新页面！", { icon: 5 });
                return false;
            }

            ck = this.checkSuffix_(ext);
            if (ck == -1) {
                //console.log("浏览的文件不再后缀列表里")
                //$.tmsg("m_jfw", "该文件不支持在线阅读，请下载查看！", { infotype: 2, time_out: 1000 });
                layer.msg("该文件不支持在线阅读，请下载查看！", { icon: 5 });
            }
            else {
                if (fileUrl == "") {
                    //console.log("文件尚未转换完成，请稍后阅读");
                    //$.tmsg("m_jfw", "文件尚未转换完成，请稍后阅读！", { infotype: 2, time_out: 1000 });
                    layer.msg("文件尚未转换完成，请稍后阅读！", { icon: 5 });
                    return;
                }
                if(typeof FileViewCount=="function"){
                    FileViewCount(options.id, options.ver);
                }
                switch (ck) {
                    case "arrPdfFileExt":
                        //if ($.YH.checkBrowserVer()) {
                        //    if (window.chrome != undefined)
                        //    {
                        //        var scrollWidth = document.body.clientWidth * 0.8, scrollHeight = document.body.clientHeight * 0.9;
                        //        $.YH.box({
                        //            target: $("<div id='yh-j-pdfReader' style='width:100%;height:100%;'></div>"),
                        //            title: name,
                        //            width: scrollWidth,
                        //            height: scrollHeight,
                        //            buttonNames: [null, null],
                        //            open: function () {
                        //                $("#yh-j-pdfReader").html($('<iframe frameborder="0" id="displayPdfIframe" width="100%" height="100%" src="' +fileUrl + '"></iframe>'));
                        //            }
                        //        });
                        //    }
                        //    else{
                        //        $.YH.pdfShow(options);//调用pdf阅读插件
                        //    }
                        //} else {
                        //    //不支持pdf在线查看 调用客户端
                        //    ReadPdfFile(fileUrl);
                        //}
                        if (options.type == 'box' && options.target!='') {
                            $YH.pdfShow(options);
                        } else {
                            ReadPdfFile(fileUrl,name);
                        }
                        break;
                    case "arrExcelFileExt":
                        ReadExcelFile(fileUrl, name);
                        break;
                    case "arrImageFileExt":
                        $.YH.imageShow(options,obj);
                        break;
                    case "arrDwgFileExt":
                        //console.log("dwg");
                        ReadDwgFile(fileUrl,name);
                        break;
                    case "arrVedioFileExt":
                        ReadVedioFile(fileUrl,name);
                        break;
                    default:  //暂不支持的文件类型
                        this.needToDownLoad_();
                        alert('default:' + options.type);
                }
            }
        },
        checkBrowserVer: function () { //method:判读浏览器是否为IE9以下版本
            var support;
            if (window.addEventListener) {
                support = true;  //IE9及以上浏览器 FF chrome
            } else if (window.attachEvent) {
                support = false; //IE9以下版本
            }
            return support;
        },
        checkSuffix_: function (ext) { //method:判读原始文件拓展名
            if (ext == "") return -1;
            var key, arr, idx;
            for (key in this.suffixClass) {
                arr = this.suffixClass[key];
                idx = arr.indexOf(ext.toLowerCase());
                if (idx != -1) {
                    return key;
                }
            }
            return -1;
        },
        needToDownLoad_: function () {//method:需要下载阅读
            //console.log("需要下载查看！");
            $.tmsg("m_jfw", "该文件不支持在线阅读，请下载查看！", { infotype: 2, time_out: 1000 });
        },
        imageShow: function (file, obj) {
            if (!file || !file.fileUrl) {
                //alert("该图片不存在！");
                $.tmsg("m_jfw", "图片不存在！", { infotype: 2, time_out: 1000 });
                return false;
            }
            var imgHtml = $('<a href="' + file.fileUrl + '" onclick="return hs.expand(this);" class="highslide"></a><div class="highslide-caption">' + file.name + file.ext + '</div>');
            $(top.document.body).append(imgHtml);
            imgHtml.trigger("click");
        },
        pdfShow: function (file) {
            if (!file || !file.fileUrl) {
                //alert("该文件不存在！");
                $.tmsg("m_jfw", "文件不存在！", { infotype: 2, time_out: 1000 });
                return false;
            }
            if (!file.target && !file.type) {
                console.info("请配置加载对象！");
                return false;
            }
            var fileName = fileName || "文件阅读", scrollWidth = document.body.clientWidth * 0.8, scrollHeight = document.body.clientHeight*0.9;
            var $pdfHtml = $('<iframe frameborder="0" id="displayPdfIframe" width="100%" height="100%" src="http://192.168.1.178:9034/pdf.js-master/web/viewer.html?file=' +
                 encodeURIComponent(file.fileUrl) + '"></iframe>');

            if (file.type =="box" && !file.target) {
                $.YH.box({
                    target: $("<div id='yh-j-pdfReader' style='width:100%;height:100%;'></div>"),
                    title:fileName,
                    width: scrollWidth,
                    height: scrollHeight,
                    buttonNames: [null, null],
                    open: function () {
                        $("#yh-j-pdfReader").html($pdfHtml);
                    }
                });
            }
        }
    });
    $.extend($.YH.readOnline, {
        info: '文件在线阅读',
        url: '',
        defaults: {
            "guid": '',    //文件id
            "name": '',    //文件名
            "ext": '',     //拓展名
            "id": '',      //
            "type": '',    //加载类型 box 或者加载到页面中（html）
            "ver": '',     //
            "swfUrl": '',
            "fileUrl": '', //文件路径
            "target": ''
        },
        options: {
            "guid": '',
            "name": '（必填）文件名',
            "ext": '（必填）文件扩展名',
            "id": '',
            "type": '',
            "ver": '',
            "swfUrl": '',
            "fileUrl": '（必填）文件路径',
            "target":'(当加载对象为pdf时为必填)加载pdf阅读模块目标'
        }
    });
})(jQuery, window);
/**
**********************************************************
* by qinghuai.huang 2016.11.11
* 合并单元格
* 用法：selector.madeRowspan({参数});
**********************************************************
*/
; (function ($) {
    $.fn.madeRowspan = function (options) {
        var opts = $.extend({}, {
            cols: [0] //要进行合并的列 0表示第一列
        }, options);
        $.fn.madeRowspan.options = opts;
        // console.log(opts)
        var $table = $(this), cols = opts.cols,$targetTr = $table.children('tbody').children('tr');
        $table.data('col-content', '');     // 存放单元格内容
        $table.data('col-rowspan', 1);      // 存放计算的rowspan值 默认为1
        $table.data('col-td', $());         // 存放发现的第一个与前一行比较结果不同td(jQuery封装过的), 默认一个"空"的jquery对象
        $table.data('trNum', $targetTr.length);     // 要处理表格的总行数, 用于最后一行做特殊处理时进行判断之用
        if (cols != null) {
            for (var i = cols.length - 1; cols[i] != undefined; i--) {//合并单元格
                $targetTr.each(function (index) {
                    var $tr = $(this);
                    var $td = $('td:eq(' + cols[i] + ')', $tr);
                    var currentContent = $td.html();

                    if ($table.data('col-content') == '') {
                        $table.data('col-content', currentContent);
                        $table.data('col-td', $td);
                    } else {
                        // 上一行与当前行内容相同
                        if ($.trim($table.data('col-content')) == $.trim(currentContent) && $.trim(currentContent)!=="") {
                            var rowspan = $table.data('col-rowspan') + 1;
                            $table.data('col-rowspan', rowspan);
                            $td.hide();

                            if (++index == $table.data('trNum')) {//table 最后一行
                                $table.data('col-td').attr('rowspan', $table.data('col-rowspan')).show();
                            } else {
                                $table.data('col-td').attr('rowspan', $table.data('col-rowspan')).show();
                            }
                        } else {
                            if ($table.data('col-rowspan') != 1) {
                                $table.data('col-td').attr('rowspan', $table.data('col-rowspan')).show();
                            }
                            // 保存第一次出现不同内容的td, 和其内容, 重置col-rowspan
                            $table.data('col-td', $td);
                            $table.data('col-content', $td.html());
                            $table.data('col-rowspan', 1);
                        }
                    }
                });
            }
            $targetTr.each(function(){
                var $tr = $(this);
                for(var i=0;i<opts.cols.length;i++){
                    var tdOne = $tr.find('td:eq('+opts.cols[i]+')'),tdSec = tdOne.next();
                    if($.trim(tdOne.text()) == $.trim(tdSec.text()) && $.trim(tdOne.text())!== ""){
                        tdOne.attr('colspan',+(tdOne.attr('clospan')||1)+1).attr('align','center');
                        tdSec.hide();
                    }
                }
            });
        }
    }
    $.fn.madeRowspan2 = function (options) {
        var opts = $.extend({}, {
            cols: [0] //要进行合并的列 0表示第一列
        }, options);
        $.fn.madeRowspan.options = opts;
        // console.log(opts)
        var $table = $(this), cols = opts.cols,$targetTr = $table.children('tbody').children('tr');
        $table.data('col-content', '');     // 存放单元格内容
        $table.data('col-rowspan', 1);      // 存放计算的rowspan值 默认为1
        $table.data('col-td', $());         // 存放发现的第一个与前一行比较结果不同td(jQuery封装过的), 默认一个"空"的jquery对象
        $table.data('trNum', $targetTr.length);     // 要处理表格的总行数, 用于最后一行做特殊处理时进行判断之用
        if (cols != null) {
            for (var i = cols.length - 1; cols[i] != undefined; i--) {//合并单元格
                $targetTr.each(function (index) {
                    var $tr = $(this);
                    var $td = $('td:eq(' + cols[i] + ')', $tr);
                    var currentContent = $td.html();

                    if ($table.data('col-content') == '') {
                        $table.data('col-content', currentContent);
                        $table.data('col-td', $td);
                    } else {
                        // 上一行与当前行内容相同
                        if ($.trim($table.data('col-content')) == $.trim(currentContent) && $.trim(currentContent)!=="") {
                            var rowspan = $table.data('col-rowspan') + 1;
                            $table.data('col-rowspan', rowspan);
                            $td.hide();

                            if (++index == $table.data('trNum')) {//table 最后一行
                                $table.data('col-td').attr('rowspan', $table.data('col-rowspan')).show();
                            } else {
                                $table.data('col-td').attr('rowspan', $table.data('col-rowspan')).show();
                            }
                        } else {
                            if ($table.data('col-rowspan') != 1) {
                                $table.data('col-td').attr('rowspan', $table.data('col-rowspan')).show();
                            }
                            // 保存第一次出现不同内容的td, 和其内容, 重置col-rowspan
                            $table.data('col-td', $td);
                            $table.data('col-content', $td.html());
                            $table.data('col-rowspan', 1);
                        }
                    }
                });
            }
            $targetTr.each(function(){
                var $tr = $(this);
                for(var i=0;i<opts.cols.length;i++){
                    var tdOne = $tr.find('td:eq('+opts.cols[i]+')'),tdSec = tdOne.next();
                    if($.trim(tdOne.text()) == $.trim(tdSec.text()) && $.trim(tdOne.text()) == ""){
                        tdOne.attr('colspan','2');
                        tdSec.hide();
                    }
                }
            });
        }
    }
})(jQuery);
/**
**********************************************************
* by qinghuai.huang 2017.05.24
* 分割表格
* 用法：selector.splitTable({参数});
**********************************************************
*/
; (function ($) {
    $.fn.splitTable = function (options) {
        var opts = $.extend({}, {
            cols: [],
            colsFlag: 0
        }, options);
        $.fn.splitTable.options = opts;

        var $table = $(this), targetTr = $table.children('tbody').children('tr'), targetLen = targetTr.length,
            tableHead = $table.find('thead'), tableCol = $table.find('colgroup');
        var isEven = $table.find('tbody tr').length%2 == 0? true: false;

        if(opts.colsFlag>0){
            var colsAry = [];
            var cols = $table.find('tbody tr:first').find('td').length;
            for(var i=0;i<opts.colsFlag;i++){
                colsAry[colsAry.length] = i;
                colsAry[colsAry.length] = +i+cols;
            }
            opts.cols = colsAry;
        }

        tableCol.append(tableCol.html())
        tableHead.find('tr').append(tableHead.find('tr').html())
        if(isEven){//偶数的 直接平分
            var secondPart = $(targetTr[targetLen/2-1]).nextAll().clone();
            $(targetTr[targetLen/2-1]).nextAll().remove();
            secondPart.each(function(i, tr){
                $(targetTr[i]).append($(tr).html());
            });
        }else{
            var secondPart = $(targetTr[Math.floor(targetLen/2)]).nextAll().clone();
            $(targetTr[Math.floor(targetLen/2)]).nextAll().remove();
            secondPart.each(function(i, tr){
                $(targetTr[i]).append($(tr).html());
            });
            $table.find('tbody tr:last').append("<td colspan='6'></td>");
        }
        if(opts.cols.length>0){
            $table.madeRowspan({cols: opts.cols});
        }
    }
})(jQuery);
/**
**********************************************************
* by qinghuai.huang 2017.3.15
* 圆环百分比
* depend Raphaël.js
* 用法：selector.circleBar({参数});
* selector 上必须的参数为 num="数值" /目前只支持0-100的数
**********************************************************
*/
; (function ($) {
    "use strict"
    $.fn.circleBar = function (options) {
        return this.each(function(){
            var opts = $.extend({}, {
                radius: 25,  //半径
                ringWidth:5, //圆环宽度
                colorStaff:{ //颜色区间
                    0: '#dddddd',
                    11: '#0aa699',
                    33: '#0090d9',
                    55: '#826f96',
                    77: '#f39d58',
                    100: '#f35958'
                },
                circleBarBg: '#ddd', // 底下圆环颜色（未充满的部分）
                unit: '%'
            }, options);
            var self = $(this),
                Id = Math.ceil(Math.random() * 9999)+String.fromCharCode(65+Math.ceil(Math.random() * 25)),//随机生成的ID
                circleId = "id"+Id,textId = "txt"+Id;
            self.html("<div id='" + circleId + "'></div><div id='"+textId+"'></div>");
            var init = function () {
                //初始化Raphael画布
                var width = +opts.radius*2 + 4,paper = null;
                paper = Raphael(circleId, +width, +width);
                //地图
                //paper.image("progressBg.png", 0, 0, width, width);
                //PS：不支持画100%，要按99.99%来画
                var percent = parseFloat(self.attr('num').replace(/%/g,"")/100).toFixed(2),
                    drawPercent = percent >= 1 ? 0.9999 : percent;
                //r1是内圆半径，r2是外圆半径
                var r1 = opts.radius - opts.ringWidth,r2 = opts.radius,PI = Math.PI,
                    p1 = {
                        x: width/2,
                        y: width
                    },
                    p4 = {
                        x: p1.x,
                        y: p1.y - r2 + r1
                    },
                    p2 = {
                        x: p1.x + r2 * Math.sin(2 * PI * (1 - drawPercent)),
                        y: p1.y - r2 + r2 * Math.cos(2 * PI * (1 - drawPercent))
                    },
                    p3 = {
                        x: p4.x + r1 * Math.sin(2 * PI * (1 - drawPercent)),
                        y: p4.y - r1 + r1 * Math.cos(2 * PI * (1 - drawPercent))
                    },
                    path = [
                        'M', p1.x, ' ', p1.y,
                        'A', r2, ' ', r2, ' 0 ', percent > 0.5 ? 1 : 0, ' 1 ', p2.x, ' ', p2.y,
                        'L', p3.x, ' ', p3.y,
                        'A', r1, ' ', r1, ' 0 ', percent > 0.5 ? 1 : 0, ' 0 ', p4.x, ' ', p4.y,
                        'Z'
                    ].join(''),

                    p21 = {
                        x: p1.x + r2 * Math.sin(2 * PI * (0.001)),
                        y: p1.y - r2 + r2 * Math.cos(2 * PI * (0.001))
                    },
                    p31 = {
                        x: p4.x + r1 * Math.sin(2 * PI * (0.001)),
                        y: p4.y - r1 + r1 * Math.cos(2 * PI * (0.001))
                    },
                    path2 = [
                        'M', p1.x, ' ', p1.y,
                        'A', r2, ' ', r2, ' 0 ', 1, ' 1 ', p21.x, ' ', p21.y,
                        'L', p31.x, ' ', p31.y,
                        'A', r1, ' ', r1, ' 0 ', 1, ' 0 ', p4.x, ' ', p4.y,
                        'Z'
                    ].join('');
                
                //下层圆环
                paper.path(path2)
                    .attr({ "stroke-width": 0.5, "stroke": opts.circleBarBg, "fill": opts.circleBarBg });
                //百分比圆环
                if(percent>0){
                    var circleBarColor= "";
                    for(var i in opts.colorStaff){
                        if(percent*100>i) continue;
                        if(percent*100<=i){ 
                            circleBarColor = opts.colorStaff[i];
                            break;
                        }
                    }
                    paper.path(path)
                        .attr({ "stroke-width": 0.5, "stroke": circleBarColor, "fill": circleBarColor });
                }
                
                //显示进度文字
                if(opts.radius >= 20){
                    $("#" + textId).text(Math.round(percent * 100) + opts.unit).css({
                        "width": width+"px",
                        "height": width+"px",
                        "line-height": width+"px",
                        "position": "absolute",
                        "margin-top": -width+"px",
                        "text-align": "center",
                        "color": "#9e9fa3",
                        "font-size": "14px",
                        "font-family": "Arial"            
                    });
                }else{
                //<div class='yh-tooltip-arrow' style='top:-5px;left: 20px;margin-left: -5px;border-bottom-color: #000;border-width: 0 5px 5px;'></div>
                    $("#"+textId).hide().addClass("yh-tooltip fade in")
                        .append("<div class='yh-tooltip-inner'>"+Math.round(percent * 100) + "%</div>");
                    self.hover(function(){$("#"+textId).show();},function(){$("#"+textId).hide();});
                }
            }
            init();
        });
    }
})(jQuery);

;(function(){
    if(!-[1,]){
        $(document).on('click',".p-j-showSelect4Ie select",function(){
            var self = $(this);
            self.css("width","auto");
            self.off('blur').on('blur',function(){
                self.css("width",self.closest('.p-j-showSelect4Ie').css('width'));
            });
        });
    }
})();
/**
**********************************************************
* by qinghuai.huang 2017.3.15
* 百度地图智能提示
* depend 百度地图API
* 用法：selector.bmapAutoComplite({参数});
**********************************************************
*/
; (function ($) {
    $.fn.bmapAutoComplite = function (options) {
        var opts = $.extend({}, {
            map: null, // 地图对象
            clickItem: $.noop  //点击搜索项 回调
        }, options);
        
        if(!BMap) return false;
        var self = $(this),map = opts.map;
        var ac = new BMap.Autocomplete({
                "input": self.prop('id'),
                "location": map,
                "onSearchComplete": osc
            });
        var srp = $('<div id="searchResultPanel" style="border:1px solid #C0C0C0;width:150px;height:auto; display:none;"></div>');
        var rspStr = '<ul id="autoComplete" style="display:none;position:absolute;background:#fff;padding:0 3px;'+
                     'max-height:200px;overflow-y:scroll;list-style: none;z-index:10000;border:1px solid #ddd;width:'+(+self.width()+50)+'px;"></ul>';
        var rsp = $(rspStr);
        self.after(srp).after(rsp);
        self.parent().css('position','relative');
        
        function osc() {
            ac.hide();
            var aResults = ac.getResults(), //获取结果对象数组
                sList = [],rs;
            for (var p in aResults) {
                Object.prototype.toString.call(aResults[p]) == "[object Array]" && (rs = aResults[p]);
            }
            
            for (var i = 0; i < rs.length; i++) {
                var o = rs[i];
                sList.push("<li style='border-bottom:1px dashed #bbb;'>", o.city, o.district, o.business, "</li>\n");
            }
            if (sList) {
                var top = self.position().top+self.height() + (parseInt(self.css('padding-top'))||0) +(parseInt(self.css('padding-bottom'))||0) + (parseInt(self.css('margin-bottom'))||0)+ (parseInt(self.css('margin-top'))||0);
                rsp.html(sList.join(' ')).css({
                    "top": top+2,
                    "left": self.position().left
                }).show();
            }
        }
        rsp.on('click', 'li', function(){opts.clickItem.call(this,self,map,rsp);});
        self.keyup(function () {
            if ($(this).val() == '') {
                rsp.html('').hide();
            }
        });
    }
})(jQuery);


(function (a) {
    a.fn.maskLG = function (b) { a(this).each(function () { a.maskElementLG(a(this), b) }) }; a.fn.unmaskLG = function () { a(this).each(function () { a.unmaskElementLG(a(this)) }) }; a.fn.isMasked = function () { return this.hasClass("masked") }; a.maskElementLG = function (b, d) {
        var e = b.data("_mask_id_LG"), g = 9999, c = b.css("position"); if (!e) {
            e = YH.genId("mask_");
            b.data("_mask_id_LG", e)
        } if (c == "absolute") g = b.css("zIndex"); if (c == "static") { b.css("position", "relative"); b.data("_mask_chgpos", "static") } b.isMasked() && a.unmaskElementLG(b); b.addClass("masked"); c = a('<div id="' + e + '" class="loadmask" style="z-index:' + (g + 5) + ';position:fixed;top:0;padding:2000px;left:0;background:rgba(0,0,0, 0.5);filter:progid:DXImageTransform.Microsoft.gradient(startColorstr=#7f000000, endColorstr=#7f000000);width:100%;height:100%;zoom:1;">' + (YH.isIE6 ? '<iframe hideFocus="true" frameborder="0" src="javascript:\'\'" style="position:absolute;z-index:-1;width:100%;height:100%;top:0px;left:0px;filter:progid:DXImageTransform.Microsoft.Alpha(opacity=0)"></iframe>' :
            "") + "</div>"); if (YH.userAgent.isIE) { c.height(b.height() + parseInt(b.css("padding-top")) + parseInt(b.css("padding-bottom"))); c.width(b.width() + parseInt(b.css("padding-left")) + parseInt(b.css("padding-right"))) } b.append(c); if (d !== undefined) {
                var h = b.offset().top; c = b.height(); var f = a(document).scrollTop(), i = YH.dom.getViewportHeight(); if (h >= f && h + c <= f + i) {
                    e = a('<div id="' + (e + "_msg") + '" style="display:none;z-index:' + (g + 6) + ';position:absolute;top:0;left:0;" class="qz_msgbox_layer_wrap"><span class="qz_msgbox_layer">' +
                        d + '<span class="gtl_ico_wait"></span><span class="gtl_end"></span></span></div>'); b.append(e); g = Math.round(c / 2 - (e.height() - parseInt(e.css("padding-top")) - parseInt(e.css("padding-bottom"))) / 2); e.css("top", g + "px"); e.css("left", Math.round(b.width() / 2 - (e.width() - parseInt(e.css("padding-left")) - parseInt(e.css("padding-right"))) / 2) + "px"); e.show()
                } else a.tmsg(e + "_msg", d, { notime: true, zindex: g + 6, infotype: 3 })
            }
    }; a.unmaskElementLG = function (b) {
        var d = b.data("_mask_id_LG"), e = b.data("_mask_chgpos"); b.find("#" + d).remove();
        a("#" + d + "_msg")[0] && a("#" + d + "_msg").remove(); b.removeClass("masked"); if (e) { b.css("position", e); b.removeData("_mask_chgpos") }
    }
})(jQuery);