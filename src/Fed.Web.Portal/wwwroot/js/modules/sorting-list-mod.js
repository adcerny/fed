
(function ($) {
    function sortingListMod() {
        var $thisLisMod = $(this),
            $toolList = $thisLisMod.find('.tool-list'),
            $userMessages = $toolList.find('.user-messages'),
            $sortingList = $thisLisMod.find('.sorting-list'),
            $sortingListElem = $sortingList.find('.list-element'),
            sortingListObj = {
                cache: {
                    $ctaTool: $toolList.find('.cta'),
                    observer: {}
                },
                data: {
                    stringToHexColor: function (str) {
                        var hash = 0,
                            colour = '#';

                        for (var i = 0; i < str.length; i++) {
                            hash = str.charCodeAt(i) + ((hash << 5) - hash);
                        }
                        
                        for (var i = 0; i < 3; i++) {
                            var value = (hash >> (i * 8)) & 0xFF;

                            colour += ('00' + value.toString(16)).substr(-2);
                        }

                        return colour;
                    }
                },
                presentation: {
                    checkToolBarState: function () {
                        if ($toolList.hasClass('fixed')) {
                            $thisLisMod.addClass('fixed-tool-bar');
                        } else {
                            $thisLisMod.removeClass('fixed-tool-bar');
                        }
                    },
                    initSortingSelectable: function (){
                        sortable = new Sortable($sortingList[0], {
                            multiDrag: true, // Enable multi-drag
                            selectedClass: 'selected', // The class applied to the selected items
                            animation: 150
                        });
                    },
                    insertMarkup: function ($context) {
                        var $markedElements = $thisLisMod.find('.list-element').filter('.marked'),
                            $cloneSelectedElem = $markedElements.clone();

                        sortable.destroy();
                        $markedElements.remove();

                        if ($context.hasClass('move-up')) {
                            $thisLisMod.find('.sorting-list').prepend($cloneSelectedElem);
                        } else if ($context.hasClass('move-bottom')) {
                            $thisLisMod.find('.sorting-list').append($cloneSelectedElem);
                        }

                        $thisLisMod.find('.list-element').filter('.marked').addClass('position-changed').removeClass('marked');

                        sortingListObj.presentation.initSortingSelectable();
                    },
                    generateProductColor: function ($context) {
                        $context.each(function () {
                            var $thisListElem = $(this),
                                $productCode = $thisListElem.text(),
                                $productCodeArray = $productCode.split('-'),
                                color = sortingListObj.data.stringToHexColor(sortingListObj.data.stringToHexColor(btoa($productCodeArray[0].trim())));

                            $thisListElem.find('.color-indicator').css({'backgroundColor': color})
                        })
                    },
                    domToArray: function ($context) {
                        var productList = [];

                        $context.each(function () {
                            productList.push($(this).text().trim());
                        });

                        return productList;
                    }
                },   
                events: function () {
                    $toolList.on('mouseenter', function () {
                        $sortingList.removeClass('mouse-out');
                        $sortingList.addClass('mouse-in');
                    })

                    $thisLisMod.find('.sorting-list-holder').on({
                        'mouseleave': function () {
                            $sortingList.addClass('mouse-out');
                            $sortingList.removeClass('mouse-in');
                        },
                        'mouseenter': function () {
                            $sortingList.addClass('mouse-in');
                            $sortingList.removeClass('mouse-out');
                        }
                    });

                    $thisLisMod.on('click', function () {
                        if ($sortingList.hasClass('mouse-out')) {
                            $thisLisMod.find('.list-element').removeClass('marked')
                        }
                    })

                    $sortingList.on('click', '.list-element' ,function () {
                        var $thisListElem = $(this);

                        if ($thisListElem.hasClass('selected')) {
                            $(this).addClass('marked');
                        } else {
                            $(this).removeClass('marked');
                        }
                    });

                    $sortingList.on('click', function () {
                        $(this).find('.selected').addClass('marked');
                    });

                    sortingListObj.cache.$ctaTool.filter('.move-bottom').add(sortingListObj.cache.$ctaTool.filter('.move-up')).on('click', function () {                                                
                        sortingListObj.presentation.insertMarkup($(this));
                    });

                    sortingListObj.cache.$ctaTool.filter('.unselect-all').on('click', function () {
                        sortable.destroy();

                        $sortingList.find('.list-element').removeClass('selected marked');

                        sortingListObj.presentation.initSortingSelectable();
                    });

                    sortingListObj.cache.$ctaTool.filter('.resort').on('click', function () {
                        var list = sortingListObj.presentation.domToArray($sortingList.find('.product-code'))

                        sortable.destroy();
                        $sortingList.html(fed.helpers.generateMarkup('sorting-list-item', { array: list.sort() }))
                        sortingListObj.presentation.generateProductColor($sortingList.find('.list-element'));
                        sortingListObj.presentation.initSortingSelectable();
                    });

                    sortingListObj.cache.$ctaTool.filter('.save').on('click', function () {
                        var productList = sortingListObj.presentation.domToArray($sortingList.find('.product-code'));

                        $.ajax({
                            url: window.location.href,
                            method: 'POST',
                            data: { productCodes: productList}
                        })
                        .done(function () {
                            $userMessages.find('.success').removeClass('hidden');
                        })
                        .catch(function () {
                            $userMessages.find('.error').removeClass('hidden');
                        }).always(function () {
                            setTimeout(function () {
                                $userMessages.children().addClass('hidden');
                            }, fed.cache.timer.long * 3)
                        })
                    });

                    sortingListObj.cache.$ctaTool.filter('.scroll-top').on('click', function () {
                        fed.cache.$body.add(fed.cache.$html).animate({
                            scrollTop: 0
                        }, fed.cache.timer.long);
                    });

                    sortingListObj.cache.$ctaTool.filter('.scroll-bottom').on('click', function () {
                        fed.cache.$body.add(fed.cache.$html).animate({
                            scrollTop: fed.cache.$body.outerHeight() - fed.cache.$window.height()
                        }, fed.cache.timer.long);
                    });

                    fed.cache.$window.on({
                        'scroll': sortingListObj.presentation.checkToolBarState,
                        'load': sortingListObj.presentation.checkToolBarState
                    });

                    fed.cache.$window.on({
                        'scroll': function () {
                            fed.helpers.fixedHeader(fed.cache.$sortingListMod.find('.tool-list'));
                        },
                        'load': function () {
                            fed.helpers.fixedHeader(fed.cache.$sortingListMod.find('.tool-list'));
                        }
                    });
                },
                init: function (){
                    sortingListObj.presentation.initSortingSelectable();

                    sortingListObj.presentation.generateProductColor($sortingListElem);

                    sortingListObj.events();
                }
            },
            sortable = {};

        sortingListObj.init();
    }

    // modules init
    $(document).ready(function (){
        fed.cache.$sortingListMod.each(sortingListMod)
    });
})(jQuery);