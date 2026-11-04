let map, draw, layer, measure, featureType, wmsSource;
let modify, select, snap, dragBox;
let infoStatus = false;
let distanceStatus = false;
let doubleClickZoomInteraction;
let infoModifyStatus = false;
let wmsLayers = [];

let osmSource;

$.fn.bootstrapTable.locales['tr-TR'] = {
    formatLoadingMessage: function () { return 'Yükleniyor, lütfen bekleyin...'; },
    formatRecordsPerPage: function (pageNumber) { return pageNumber + ' kayıt / sayfa'; },
    formatShowingRows: function (from, to, total) {
        return (from === to ? from : from + ' - ' + to) + ' / ' + total + ' kayıt gösteriliyor';
    },
    formatSearch: function () { return 'Ara'; },
    formatNoMatches: function () { return 'Eşleşen kayıt bulunamadı'; },
    formatRefresh: function () { return 'Yenile'; },
    formatToggle: function () { return 'Görünümü değiştir'; },
    formatColumns: function () { return 'Sütunlar'; },
    formatPaginationSwitch: function () { return 'Sayfalama görünümünü değiştir'; },
    formatDetailPagination: function (totalRows) { return 'Toplam kayıt: ' + totalRows; },
    formatClearSearch: function () { return 'Aramayı temizle'; },
    formatFullscreen: function () { return 'Tam ekran'; },
    formatAllRows: function () { return 'Tümü'; },
    formatPrint: function () { return 'Yazdır'; },
    formatExport: function () { return 'Dışa aktar'; }
};
$.fn.bootstrapTable.defaults.locale = 'tr-TR';

// ---> YÜRÜTÜLME ANI BOYUNCA ÇALIŞANLAR
$(document).ready(function () {
    keyTimeWarning();

    //Projeksiyonlar ekleniyor
    projectionInserter();
    //Harita içerikleri ekleniyor
    initializeMap();

    jsPanel.errorReporting = 0; // <--- JsPanel error alertlerini kapatır

    //Buton kontrolleri
    doubleClickZoomInteraction = map.getInteractions().getArray().find(interaction => interaction instanceof ol.interaction.DoubleClickZoom);

    //Switchleri sidebar'a yazdıran ajax isteği
    $.ajax({
        url: '/Home/LayerControl',
        type: 'GET',
        data: { id: admin.id },
        success: function (data) {
            $('#sidebar').html(data);
            $('#sort-layers').on('click', function (event) {
                var sortableLayersPanel = jsPanel.create({
                    position: "center",
                    contentSize: "800 520",
                    headerTitle: "Katman Sıralama Tablosu",
                    theme: "dark",
                    id: 'sortable-layer-list-panel',
                    //clickToSelect: true,
                    checkbox: true,
                    borderRadius: '8px',
                    content: '<div class="card"><div class="card-body"><button class="btn btn-success" id="save-layer-line" title="Değişiklikleri Kaydet"> Değişiklikleri Kaydet   <i class="fa fa-check"></i></button><hr><table id="sortable-layers-list-table"></table></div></div>',

                    callback: function (panel) {
                        var dataActions = [];
                        var elements = $("input.form-check-input[data-priority]:checked");

                        dataActions = elements.map(function (index, element) {
                            return $(element).data();
                        }).get();

                        dataActions.sort(function (a, b) {
                            return a.priority - b.priority;
                        });

                        $('table#sortable-layers-list-table', panel).bootstrapTable({
                            data: dataActions,
                            //search: true,
                            //pagination: true,
                            reorderableRows: true,
                            useRowAttrFunc:true,
                            columns: [{
                                title: 'Katman Adı',
                                field: 'tag',
                                sortable: true
                            }, {
                                title: 'Grup',
                                field: 'group',
                                sortable: true
                            }, {
                                title: 'Minimum Zoom',
                                field: 'minzoom',
                                sortable: true
                            }, {
                                title: 'Maximum Zoom',
                                field: 'maxzoom',
                                sortable: true
                            }, {
                                title: 'Katman ID',
                                field: 'layerid',
                                sortable: true
                            }],
                        })

                        $('#save-layer-line', panel).off().on('click', function () {
                            wmsLayers = []
                            $.each($('#sortable-layers-list-table').bootstrapTable('getData'), function (i, k) {
                                wmsLayers.push(k.layer);
                            })


                            var sortedIndexes = $('#sortable-layers-list-table').bootstrapTable('getData');
                            $("input.form-check-input[data-priority]:checked").each(function (index, element) {
                                var tag = $(element).data('tag');
                                for (var i = 0; i < sortedIndexes.length; i++) {
                                    if (sortedIndexes[i].tag === tag) {
                                        $(element).data('priority', i);
                                        $(element).data('isopen', true)
                                        break;
                                    }
                                }
                            });

                            setLayerToggle();
                        });
                    }
                })
            });
           
            $("input.form-check-input", $("#sidebar")).off().on('change', function (event) {
                var changedLayer = $.map($("input.form-check-input:checked", $("#sidebar")), function (k, i) {
                    return $(k).data().layer
                })

                var switchDatas = event.currentTarget.dataset;
                var isChecked = event.currentTarget.checked;
                if (isChecked) {
                    $('#sortable-layers-list-table').bootstrapTable('insertRow', {
                        index: $('#sortable-layers-list-table').bootstrapTable('getData').length,
                        row: {
                            tag: switchDatas.tag,
                            group: switchDatas.group,
                            minzoom: switchDatas.minzoom,
                            maxzoom: switchDatas.maxzoom,
                            layerid: switchDatas.layerid
                        }
                    });
                }
                else {
                    $('#sortable-layers-list-table').bootstrapTable('remove', {
                        field: 'layerid',
                        values: switchDatas.layerid
                    });
                }


                wmsLayers = changedLayer.length <= 0 ? ['NO_LAYER'] : changedLayer;
                setTimeout(function () {
                    if (wmsSource) {
                        wmsSource.updateParams({ 'time': Date.now(), 'LAYERS': wmsLayers });
                    }
                }, 250);

            })
            $('#save-layer-changes').on('click', function () {
                setLayerToggle();
            })
        },
        error: function (xhr, status, error) {
            console.error('İstek başarısız: ' + xhr.responseText);
            // Hata mesajına erişmek için xhr.responseText kullanabilirsiniz
        }
    });

    // Butona tıklandığında sidebar'ı açan fonksiyon
    function toggleSidebar() {
        var sidebar = document.getElementById("sidebar");
        var navbarNav = document.querySelector(".navbar-nav");
        //var mainContent = document.getElementById("main-content");

        sidebar.classList.toggle("active");
        navbarNav.classList.toggle("active");

    }
    // Butona tıklama olayını dinle
    var button = document.getElementsByName("change-opacity")[0];
    button.addEventListener("click", toggleSidebar);


    const navButtons = $('a');
    navButtons.on('click', function (event) {
        // Tüm butonların etkinliğini kapat
        navButtons.prop('disabled', true); //Butonların disabled özellğini true yap ve düğmelerin tıklanma özelliğini kapat

        if (draw) {
            draw.setActive(false);  //Interactions'lar kapatılıyor
        }
        if (measure) {
            measure.setActive(false);
        }

        //Tıklanan butonun işlemini çalıştır
        const action = $(event.currentTarget).data('action');
        switch (action) {
            case 'get-list':
                getDataList();
                break;
            case 'select-map':
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                $(event.currentTarget).data('info-active', !infoStatus)
                infoStatus = $(event.currentTarget).data('info-active');
                break;            
            case 'draw-city':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                featureType = 'İl';
                //drawBrush("Polygon"); <--- Buradan sonrası yeni
                multiSelectedFeatures();
                select.setActive(true);
                break;
            case 'draw-county':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("Polygon");
                featureType = 'İlçe';
                break;
            case 'draw-neighborhood':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("Polygon");
                featureType = 'Mahalle';
                break;
            case 'draw-street':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("LineString");
                featureType = 'Sokak';
                break;
            case 'draw-building':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("Polygon");
                featureType = 'Bina';
                break;
            case 'draw-transformer':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("Polygon");
                featureType = 'Trafo';
                break;
            case 'draw-straight':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("Point");
                featureType = 'Direk';
                break;
            case 'draw-subscriber':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                drawBrush("Point");
                featureType = 'Abone';
                break;
            case 'measure-line':
                clearMeasureFeatures();
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                if (measure != null) {
                    measureTool('LineString');
                }

                if (measure == null || measure == undefined) {
                    measureTool('LineString');
                }
                break;
            case 'measure-polygon':
                clearMeasureFeatures();
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(this).data('distance-active', false)
                distanceStatus = $(this).data('distance-active');
                doubleClickZoomInteraction.setActive(false);
                if (measure != null) {
                    measureTool('Polygon');
                }

                if (measure == null || measure == undefined) {
                    measureTool('Polygon');
                }
                break;
            case 'get-distance':
                $(this).data('info-active', false);
                infoStatus = $(this).data('info-active');
                $(event.currentTarget).data('distance-active', !distanceStatus)
                distanceStatus = $(event.currentTarget).data('distance-active');
                break;
            case 'enter-coordinate':
                createCoordinatePanel();
                break;
        }
        //Tüm butonların etkinliğini geri aç
        navButtons.prop('disabled', false);
    });
    
    //Arama işlemi
    $('#but-search').on('click', function () {    
        var content = document.forms["search-area"]["input-searchable"].value;
        if (content == null || content == "") {
            Swal.fire({
                icon: 'warning',
                title: 'Lütfen Boş Alan Bırakmayın!',
                showConfirmButton: false,
                timer: 1500
            })
        }
        else {
            $.get('/Request/GetItemByName', {
                name: $('input[id="inp-search"]').val(),
            },
            function (records, status) {
                if (records) {
                    fitMapLayer(records)
                }
                else {
                    $.get('/Request/GetItemByNumber', {
                        number: $('input[id="inp-search"]').val(),
                    },
                    function (records, status) {
                        if (records) {
                            fitMapLayer(records)
                            didYouFind = true;
                        }
                        else {
                            Swal.fire({
                                icon: 'error',
                                title: 'Oops...',
                                text: 'Aradığınız içerik bulunamadı!',
                            })
                        }
                    })
                }
            })
        }
    });

    //Klavye işlemleri
    document.addEventListener('keydown', function (e) {
        //ESC
        if (e.keyCode === 27) {
            if (draw != null) {
                if (draw.getActive()) {
                    draw.setActive(false);
                    map.removeInteraction(draw);
                }
            }
            if (select != null) {
                if (select.getActive()) {
                    select.setActive(false);
                    map.removeInteraction(select);
                }
            }
        }
        //C
        if (e.keyCode === 67) {
            clearMeasureFeatures();
        }
        //ENTER
        if (e.keyCode === 13) {
            if (featureType === 'İl' && select.getActive() === true && infoStatus === false && distanceStatus === false) {
                var selectedFeatures = select.getFeatures().item(0);          //.getGeometry().geometries_[1].flatCoordinates
                var WKTFormat = new ol.format.WKT();
                var coordinateValue = WKTFormat.writeFeature(selectedFeatures)
                completeDrawing(selectedFeatures, coordinateValue);

                if (select != null) {
                    if (select.getActive()) {
                        select.setActive(false);
                        map.removeInteraction(select);
                    }
                }
            }
        }
        //BACKSPACE
        if (e.keyCode === 8) {
            const kmlLayer = map.getLayers().getArray().find(layer => layer instanceof ol.layer.Vector && layer.get('title') === 'loaded-kml-file');
            if (kmlLayer) {
                map.removeLayer(kmlLayer);
            }
        }
    })

    //Panel(ler) kapandığı anda yapılmasını istediğim işlemleri buraya yazıyorum. 
    let handler = function (event) {
        console.log("PANEL KAPANDI!");

        mapInfoList = [];   //Harita bilgi listesini temizle

        source.getFeatures().forEach(function (item) {
            if ("entry-panel-" + item.ol_uid == event.detail) {
                source.removeFeature(item);
            }

            //Güncelleme ekranında değişiklikler kaydedilmediyse...
            if (item.getId() == event.detail) {
                cancelGeometryChanges(item);
            }
        })

        if (event.detail == 'multi-select-panel') {
            dragBox.setActive(true);
            select.getFeatures().clear();
        }
    }
    document.addEventListener('jspanelclosed', handler, false);

    //KML dosyası seçme işlemi için
    $("#input-file-selector").on("change", function () {
        const file = this.files[0];
        if (file) {
            var reader = new FileReader();
            reader.onload = function () {
                var newLayer = new ol.layer.Vector({
                    source: new ol.source.Vector({
                        url: reader.result,
                        format: new ol.format.KML()
                    }),
                    title: "loaded-kml-file"
                });
                map.addLayer(newLayer);
            }
            reader.readAsDataURL(file);

            Swal.fire({
                title: 'KML içeriği yükleniyor...',
                html: 'Lütfen bekleyin',
                timer: 2000,
                didOpen: function() {
                    Swal.showLoading();
                },
            });
        }
    });
})
function initializeMap() {
    const container = document.getElementById('popup');
    const content = document.getElementById('popup-content');
    const closer = document.getElementById('popup-closer');

    const overlay = new ol.Overlay({
        element: container,
        autoPan: {
            animation: {
                duration: 250,
            },
        },
    });

    closer.onclick = function () {
        overlay.setPosition(undefined);
        closer.blur();
        return false;
    };

    //////////////////////////////////////////////////////////////////////////
    osmSource = new ol.source.OSM();
    const geoServerBaseUrl = (window.geoServerBaseUrl || '').trim().replace(/\/+$/, '');
    wmsSource = null;
    wmsLayers = [];
    if (geoServerBaseUrl) {
        wmsSource = new ol.source.TileWMS({
            url: `${geoServerBaseUrl}/wms`,
            params: {
                'LAYERS': wmsLayers,
                'TILED': true,
                'VERSION': '1.1.1',
                'FORMAT': 'image/png'
            },
            serverType: 'geoserver'
        });

        $.get('/Request/GetLayerList', { id: admin.id }, function (records) {
            wmsLayers = Array.isArray(records) ? records : [];
            wmsSource.updateParams({ 'time': Date.now(), 'LAYERS': wmsLayers });
        });
    }

    source = new ol.source.Vector({ wrapX: false });

    const mapLayers = [new ol.layer.Tile({ source: osmSource, title: 'osm' })];
    if (wmsSource) {
        mapLayers.push(new ol.layer.Tile({ source: wmsSource, title: 'wms' }));
    }
    mapLayers.push(new ol.layer.Vector({ source: source }));

    map = new ol.Map({
        overlays: [overlay],
        view: new ol.View({
            projection: ol.proj.get('EPSG:3857')
        }),
        target: 'map',
        layers: mapLayers,
    });

    //////////////////////////////////////////////////////////////////////////

    //Her çözünürlükte Türkiye'ye ortalanması için koordinat değerlerini giriyorum
    var extent = [2867005.99808373, 4251751.330229877, 5010777.341726353, 5209567.7405384425];
    map.getView().fit(extent, map.getSize());

    //Etkileşim araçlarını projeye ekliyorum ve lazım olması durumunda açmak için kapalı hale getiriyorum
    map.addInteraction(select);
    map.addInteraction(modify);
    map.addInteraction(snap);
    map.addInteraction(dragBox);
    select.setActive(false);
    modify.setActive(false);
    snap.setActive(false);
    dragBox.setActive(true);

    //İşaretçileri değiştiriyorum
    map.on("pointermove", function (e) {
        if (!infoStatus && !distanceStatus) {
            this.getTargetElement().style.cursor = '';
            return;
        }
        //Seçme butonu kontrolü
        if (infoStatus) {
            this.getTargetElement().style.cursor = 'crosshair';
        }
        //Ölçme butonu kontrolü
        if (distanceStatus) {
            this.getTargetElement().style.cursor = 'crosshair';
        }
    });
    //Map Info--------------------------------------------------------------------------------------
    map.on('singleclick', function (e) {
        var zoomLevel = map.getView().getZoom();
        zoomLevel = zoomLevel.toFixed(1);
        var WKTFormat = new ol.format.WKT();
        var pointGeom = new ol.geom.Point(e.coordinate);
        var wkt = WKTFormat.writeGeometry(pointGeom);
        console.log(wkt);
        console.log(zoomLevel);

        let layersString = layers.toString();

        if (!infoStatus) {
            return;
        }
        $.get("/Request/GetInfoFromMap",
            {
                wkt: wkt,
                layers: layersString,
                zoomLevel: zoomLevel
            },
            function (intersectList) {
                console.log(intersectList);

                if (intersectList.length > 0) {
                    console.log("A feature got hit!");
                    jsPanel.create({
                        position: "center",
                        contentSize: "auto",
                        headerTitle: "Harita Bilgi Listesi",
                        theme: "dark",
                        id: 'list-panel-' + e.pixel[0],
                        //clickToSelect: true,
                        checkbox: true,
                        borderRadius: '8px',
                        content: '<table id="Map-Records-Table"></table>',

                        callback: function (panel) {
                            $('#Map-Records-Table', panel).bootstrapTable({
                                data: intersectList,
                                height: 500,
                                search: true,
                                pagination: true,
                                columns: [{
                                    title: 'ID',
                                    field: 'id',
                                    sortable: true
                                }, {
                                    title: 'Tablo Adı',
                                    field: 'featureType'
                                }, {
                                    title: 'İşlevler',
                                    width: '150px',
                                    formatter: function (value, row, index) {
                                        return '<div class="btn-group" role="group" aria-label="İşlevler"><button type="button" class="btn btn-primary get-info" title="Bilgi" aria-label="Bilgi"><i class="fa fa-info" aria-hidden="true"></i></button><button type="button" class="btn btn-warning view-object" title="Haritada göster" aria-label="Haritada göster"><i class="fa fa-eye" aria-hidden="true"></i></button></div>';
                                    },
                                    events: {
                                        'click .get-info': function (e, value, row, index) {
                                            rowInfoPanel(row, index, '#Map-Records-Table');
                                        },
                                        'click .view-object': function (e, value, row, index) {
                                            fitMapLayer(row);
                                        }
                                    }
                                }],
                            })
                        }
                    })
                }
                else {
                    Swal.fire({
                        icon: 'question',
                        title: 'İçerik Bulunamadı...',
                        text: 'İçeriği seçtiğinden emin ol!',
                        showConfirmButton: false,
                        timer: 1000
                    })
                }
            }
        );
    });
    //Ölçümleri Kontrol Et--------------------------------------------------------------------------
    map.on('singleclick', function (e) {
        if (!distanceStatus || !wmsSource) {
            return;
        }

        var viewResolution = map.getView().getResolution();

        var url = wmsSource.getFeatureInfoUrl(
            e.coordinate,
            viewResolution,
            'EPSG:3857',
            { 'INFO_FORMAT': 'application/json' }
        );
        if (url) {
            fetch(url)
                .then(function (response) {
                    return response.json();
                })
                .then(function (json) {
                    console.log(json);
                    var features = new ol.format.GeoJSON().readFeatures(json);
                    content.innerHTML = calculateMeasurement(features[0]);
                    overlay.setPosition(e.coordinate);
                });
        }
    });
    //----------------------------------------------------------------------------------------------
}

// ---> TOOL'LAR BU BÖLÜMDE BULUNUR
function createSelect() {
    return new ol.interaction.Select();
}
function createModify() {
    var mod = new ol.interaction.Modify({
        features: select.getFeatures()
    });

    mod.on("modifyend", (event) => {
        var WKTFormat = new ol.format.WKT();
        var targetFeature = event.features.array_[0];
        var coordinateValue = WKTFormat.writeFeature(targetFeature);
        var measureValue = calculateMeasurement(event.features.array_[0]);

        var x = $('#' + targetFeature.getId()).find('[name="coordinate"]');
        x.val(coordinateValue);

        var y = $('#' + targetFeature.getId()).find('[name="measurement"]');
        y.val(measureValue)
    });
    return mod;
}
function createSnap() {
    return new ol.interaction.Snap({ features: select.getFeatures() });
}
function createDragBox() {   
    return new ol.interaction.DragBox({ condition: ol.events.condition.platformModifierKeyOnly })
}
select = createSelect();
modify = createModify();
snap = createSnap();
dragBox = createDragBox();

// ---> ÇİZİM VE KAYIT İŞLEMLERİ BU BÖLÜMDE YAPILMAKTADIR
function drawBrush(typeOf) {
    draw = new ol.interaction.Draw({
        source: source,
        type: typeOf,
    });
    map.addInteraction(draw);
    snap.setActive(true);
    draw.on("drawend", (event) => {
        var WKTFormat = new ol.format.WKT();
        var coordinateValue = WKTFormat.writeFeature(event.feature)
        var feature = event.feature;
        console.log("Yeni Veri Koordinatları: " + coordinateValue);
        completeDrawing(feature, coordinateValue);
        setTimeout(function () {
            doubleClickZoomInteraction.setActive(true);
        }, 1000);
    });
}
function completeDrawing(event, coordinate) {
    if (draw) {
        draw.setActive(false);
    }
    let dataEntryPanel = jsPanel.create({
        position: "center",
        contentSize: "auto",
        headerTitle: featureType + " Kayıt Paneli",
        theme: "dark",
        id: "entry-panel-" + event.ol_uid,
        borderRadius: '8px',

        contentAjax: {
            url: requestString(featureType, '/Entry/', 'Entry'),
            done: function (xhr, panel) {
                panel.contentRemove();
                panel.content.append(jsPanel.strToHtml(xhr.responseText));

                setInputElement(event, featureType, panel);

                $('#svfl', panel).on('click', function () {
                    post(panel, dataEntryPanel, featureType, event, coordinate);            
                })
                $('#view', panel).on('click', function () {
                    if (event.getGeometry().getType() == 'Point') {
                        map.getView().fit(event.getGeometry().getExtent(), { maxZoom: 19 });
                    }
                    else {
                        map.getView().fit(event.getGeometry().getExtent());
                    }
                })
            },
        }
    });
}

// ---> INFO PANELİ BURADA
function rowInfoPanel(row, index, recordsTable) {
    $.get(
        requestString(row.featureType, "/Request/Get", "ItemById"),
        {
            id: row.id,
        },
        function (records) {
            let dataQueryPanel = jsPanel.create({
                position: "center",
                contentSize: "auto",
                headerTitle: records.name,
                theme: "dark",
                id: 'info-panel-' + records.id,
                borderRadius: '8px',
                contentAjax: {
                    url: requestString(records.featureType, '/Query/', 'Query'),
                    done: function (xhr, panel) {
                        panel.contentRemove();
                        panel.content.append(jsPanel.strToHtml(xhr.responseText));

                        var wkt = records.coordinate;
                        var format = new ol.format.WKT();
                        feature = format.readFeature(wkt);
                        //Feature id'sine modifyend anında farklı panellerle çakışmadan erişmek için panel id'si atıyorum
                        feature.setId(panel.id);

                        $('input[name="id"]', panel).val(records.id);
                        $('input[name="street-name"]', panel).val(records.name);
                        $('input[name="street-number"]', panel).val(records.number);
                        $('input[name="located-city"]', panel).val(records.locatedCity);
                        $('textarea[name="measurement"]', panel).val(records.measurement);
                        $('textarea[name="coordinate"]', panel).val(records.coordinate);

                        setInputElement(records, records.featureType, panel);

                        $('#upfl', panel).on('click', function () {
                                Swal.fire({
                                    title: 'Güncellemek istediğinize emin misiniz?',
                                    text: "Bu işlem verilerinize zarar vermez.",
                                    icon: 'warning',
                                    showCancelButton: true,
                                    confirmButtonColor: '#3085d6',
                                    cancelButtonColor: '#d33',
                                    confirmButtonText: 'Evet, Güncelle!'
                                }).then((result) => {
                                    if (result.isConfirmed) {
                                        var format = new ol.format.WKT();
                                        var feature = format.readFeature(records.coordinate);
                                        var event = feature;
                                        put(panel, dataQueryPanel, records, event, index, recordsTable)
                                    } else {
                                        Swal.fire('Değişiklikler kaydedilmedi', '', 'info')
                                    }
                                })
                        });
                        $('#delfl', panel).on('click', function () {
                            deleteFeature(row, recordsTable, dataQueryPanel)
                        });
                        $('#cancel', panel).on('click', function () {
                            $.get(requestString(records.featureType, "/Request/Get", "ItemById"),
                                {
                                    id: row.id,
                                },
                                function (data) {
                                    $('input[name="id"]', panel).val(data.id);
                                    $('input[name="street-name"]', panel).val(data.name);
                                    $('input[name="street-number"]', panel).val(data.number);
                                    $('input[name="located-city"]', panel).val(data.locatedCity);
                                    $('textarea[name="coordinate"]', panel).val(data.coordinate);
                                }
                            );
                        });
                        $('#modify', panel).on('click', function () {
                            infoModifyStatus = !($(this).data('modify-active'));
                            $(this).data('modify-active', infoModifyStatus);
                            console.log(infoModifyStatus);

                            if (infoModifyStatus) {
                                source.addFeature(feature);
                                fitMapLayer(row);

                                select.getFeatures().remove(feature);
                                select.getFeatures().push(feature);
                                modify.setActive(true);
                                snap.setActive(true);
                            } else {
                                //Kaydedilmeden modify butonuna tekrar tıklanırsa feature'ı sil
                                source.removeFeature(feature);
                                
                                select.setActive(false);
                                modify.setActive(false);
                                snap.setActive(false);
                            }
                        });
                        $('#view', panel).on('click', function () {
                            fitMapLayer(row);
                        });
                    },
                }
            });
        }
    )
}

// ---> LİSTELER BURADA OLUŞTURULUR
function createList(panelId, content, recordsTable, data) {
    jsPanel.create({
        position: "center",
        contentSize: "750 750",
        headerTitle: "Bilgi Listesi",
        theme: "dark",
        id: panelId,
        borderRadius: '8px',
        content: content,

        callback: function (panel) {
            $(recordsTable, panel).bootstrapTable({
                data: data,
                height: 750,
                url: '/Request/GetSortedList',
                search: true,
                pagination: true,
                sidePagination: 'server',
                columns: [{
                    title: 'ID',
                    field: 'id',
                    sortable: true
                }, {
                    title: 'Adı',
                    field: 'name'
                }, {
                    title: 'Numarası',
                    field: 'number'
                }, {
                    title: 'Bulunduğu İl',
                    field: 'locatedCity'
                }, {
                    title: 'Tür',
                    field: 'featureType'
                }, {
                    searchable: false,
                    title: 'İşlevler',
                    field: 'noSearchable',
                    width: '150px',
                    formatter: function (value, row, index) {
                        return '<div class="btn-group" role="group" aria-label="İşlevler"><button type="button" class="btn btn-primary get-info" title="Bilgi" aria-label="Bilgi"><i class="fa fa-info" aria-hidden="true"></i></button><button type="button" class="btn btn-danger delete-record" title="Sil" aria-label="Sil"><i class="fa fa-trash-o" aria-hidden="true"></i></button><button type="button" class="btn btn-warning view-object" title="Haritada göster" aria-label="Haritada göster"><i class="fa fa-eye" aria-hidden="true"></i></button></div>';
                    },
                    events: {
                        'click .get-info': function (e, value, row, index) {
                            rowInfoPanel(row, index, recordsTable);
                        },
                        'click .delete-record': function (e, value, row, index) {
                            deleteFeature(row, recordsTable);
                        },
                        'click .view-object': function (e, value, row, index) {
                            fitMapLayer(row);
                        }
                    }
                }],
            })
        }
    })
}
function getDataList() {
    $.get("/Request/GetSortedList", function (records, status) {
        createList('list-panel-1', '<table id="Records-Table"></table>', '#Records-Table', records)
    })
}

// ---> MEAUSRE İŞLEMLERİ BURADA YAPILIR
function calculateMeasurement(eventFeature) {
    var geometryType = eventFeature.getGeometry().getType();

    if (geometryType === 'Polygon' || geometryType === 'GeometryCollection') {
        const area = ol.sphere.getArea(eventFeature.getGeometry());
        let output;
        if (area > 10000) {
            output = Math.round((area / 1000000) * 100) / 100;
            return "AREA: " + output + ' ' + 'km²';
        } else {
            output = Math.round(area * 100) / 100;
            return "AREA: " + output + ' ' + 'm²';
        }
    }
    else if (geometryType === 'LineString') {
        const length = ol.sphere.getLength(eventFeature.getGeometry());
        let output;
        if (length > 100) {
            output = Math.round((length / 1000) * 100) / 100;
            return "LENGTH: " + output + ' ' + 'km';
        } else {
            output = Math.round(length * 100) / 100;
            return "LENGTH: " + output + ' ' + 'm';
        }
        
    }
    else if (geometryType === 'Point') {    //Koordinat bilgilerinin yanlış gösterilmemesi için sistem değişikliğine gidilmiştir.
        var lonlat = ol.proj.transform(eventFeature.getGeometry().getCoordinates(), 'EPSG:3857', 'EPSG:4326');
        return "LONLAT: " + 'EPSG:4326(' + lonlat + ')';
    }
    else {
        console.log("Hatalı geometri??");
        return "?";
    }
}
function measureTool(featureType) {
    let sketch;
    let measureTooltipElement;
    let measureTooltip;

    const typeSelect = $('#navbarDropdown1');

    const formatLength = function (line) {
        //Math.round sayıyı en yakın tamsayıya yuvarlar
        const length = ol.sphere.getLength(line);
        let output;
        if (length > 100) {
            output = Math.round((length / 1000) * 100) / 100 + ' ' + 'km';
        } else {
            output = Math.round(length * 100) / 100 + ' ' + 'm';
        }
        return output;
    };
    const formatArea = function (polygon) {
        const area = ol.sphere.getArea(polygon);
        let output;
        if (area > 10000) {
            output = Math.round((area / 1000000) * 100) / 100 + ' ' + 'km<sup>2</sup>';
        } else {
            output = Math.round(area * 100) / 100 + ' ' + 'm<sup>2</sup>';
        }
        return output;
    };

    function addInteraction() {
        measure = new ol.interaction.Draw({
            source: source,
            type: featureType,
            style: new ol.style.Style({
                fill: new ol.style.Fill({
                    color: 'rgba(255, 255, 255, 0.2)',
                }),
                stroke: new ol.style.Stroke({
                    color: 'rgba(0, 0, 0, 0.5)',
                    lineDash: [10, 10],
                    width: 2,
                }),
                image: new ol.style.Circle({
                    radius: 5,
                    stroke: new ol.style.Stroke({
                        color: 'rgba(0, 0, 0, 0.7)',
                    }),
                    fill: new ol.style.Fill({
                        color: 'rgba(255, 255, 255, 0.2)',
                    }),
                }),
            }),
        });
        map.addInteraction(measure);

        createMeasureTooltip();

        let listener;
        measure.on('drawstart', function (evt) {
            sketch = evt.feature;

            let tooltipCoord = evt.coordinate;

            listener = sketch.getGeometry().on('change', function (evt) {
                const geom = evt.target;
                let output;
                if (geom instanceof ol.geom.Polygon) {
                    output = formatArea(geom);
                    tooltipCoord = geom.getInteriorPoint().getCoordinates();
                } else if (geom instanceof ol.geom.LineString) {
                    output = formatLength(geom);
                    tooltipCoord = geom.getLastCoordinate();
                }

                measureTooltipElement.innerHTML = output;
                measureTooltip.setPosition(tooltipCoord);
            });
        });
        measure.on('drawend', function (evt) {
            var feature = evt.feature;
            feature.setStyle(new ol.style.Style({
                fill: new ol.style.Fill({
                    color: 'rgba(255, 255, 255, 0.2)',
                }),
                stroke: new ol.style.Stroke({
                    color: '#ffcc33',
                    width: 2,
                }),
                image: new ol.style.Circle({
                    radius: 5,
                    stroke: new ol.style.Stroke({
                        color: 'rgba(0, 0, 0, 0.7)',
                    }),
                    fill: new ol.style.Fill({
                        color: '#ffcc33',
                    }),
                }),
            }));

            measureTooltipElement.className = 'ol-tooltip ol-tooltip-static';
            measureTooltip.setOffset([0, -7]);
            // unset sketch
            sketch = null;
            // unset tooltip so that a new one can be created
            measureTooltipElement = null;
            createMeasureTooltip();
            new ol.Observable(listener);

            measure.setActive(false);

            setTimeout(function () {
                doubleClickZoomInteraction.setActive(true);
            }, 1000);

            //Çizim bittiği anda feature'a diğer katmanlardan ayırt edilebilmesi için bir ol_id atadım
            feature.setId('MeasureFeature');
        });
    }

    //Ölçüleri popup olarak mouse üzerinde göster
    function createMeasureTooltip() {
        if (measureTooltipElement) {
            measureTooltipElement.parentNode.removeChild(measureTooltipElement);
        }
        measureTooltipElement = document.createElement('div');
        measureTooltipElement.className = 'ol-tooltip ol-tooltip-measure';
        measureTooltip = new ol.Overlay({
            element: measureTooltipElement,
            offset: [0, -15],
            positioning: 'bottom-center',
            stopEvent: false,
            insertFirst: false,
        });
        map.addOverlay(measureTooltip);
    }

    //Let user change the geometry type.     
    typeSelect.onchange = function () {
        map.removeInteraction(measure);
        addInteraction();
    };

    addInteraction();
}
function clearMeasureFeatures() {
    var popupOverlay = map.getOverlays().array_[0];
    source.getFeatures().forEach(function (feature) {
        if (measure) {
            if (feature.getId() == 'MeasureFeature' && !measure.getActive()) {
                source.removeFeature(feature);
                map.getOverlays().clear();
            }
        }
    });
    map.addOverlay(popupOverlay);
}

// ---> DİĞER PANELLER BURADA
function setLayerToggle() {
    var elements = $("input.form-check-input");

    elements.map(function () {
        var isChecked = $(this).prop('checked');
        $(this).data('isopen', isChecked);
    });

    $.get('/Request/DeleteOldLayerSettings',
        {
            id: userLayers[0].userId
        },
        function (result) {
            if (result) {
                var elements = $("input.form-check-input[data-priority]:checked");

                var sortableLayersList = elements.map(function (index, element) {
                    return $(element).data();
                }).get();

                $.post("/Request/PostNewLayerSettings", { layers: sortableLayersList },
                    function (records) {
                        if (records) {
                            Swal.fire({
                                position: 'center',
                                icon: 'success',
                                title: 'İçerik Kaydedildi',
                                showConfirmButton: false,
                                timer: 1500
                            })

                            setTimeout(function () {
                                if (wmsSource) {
                                    wmsSource.updateParams({ 'time': Date.now(), 'LAYERS': wmsLayers });
                                }
                            }, 250);
                        }
                        else {
                            Swal.fire({
                                position: 'center',
                                icon: 'error',
                                title: 'İçerik Kaydedilemedi!',
                                text: 'Sunucu tarafından uyumsuz bir istek gönderildi.',
                                showConfirmButton: false,
                                timer: 1500
                            })
                        }
                    }
                )
            }
        })
}
function createCoordinatePanel() {
    var coordinatePanel = jsPanel.create({
        position: 'center-top', // Sağ üst köşeye yerleştir
        borderRadius: '8px', // Panel kenar yuvarlatma
        contentSize: "auto",
        headerTitle: "Koordinat Girişi",
        theme: "white",
        id: 'coordinate-entry-panel',
        contentAjax: {
            url: '/Home/CoordinateEntry',
            done: function (xhr, panel) {
                panel.contentRemove();
                panel.content.append(jsPanel.strToHtml(xhr.responseText));

                var currentProj = 'empty';
                var currentType = 'empty';

                const listButtons = $('a', panel);
                listButtons.on('click', function (event) {
                    //Tüm butonların etkinliğini kapat
                    listButtons.prop('disabled', true); //Butonların disabled özellğini true yap ve düğmelerin tıklanma özelliğini kapat
                    //Tıklanan butonun işlemini çalıştır
                    const action = $(event.currentTarget).data('action');
                    switch (action) {
                        case 'first-proj':
                            currentProj = 'EPSG:4326'
                            $('a.dropdown-toggle[name="projection-dropdown"]').text(currentProj);
                            break;
                        case 'second-proj':
                            currentProj = 'EPSG:3857'
                            $('a.dropdown-toggle[name="projection-dropdown"]').text(currentProj);
                            break;
                        case 'usa-proj':
                            currentProj = 'EPSG:4269'
                            $('a.dropdown-toggle[name="projection-dropdown"]').text(currentProj);
                            break;
                        case 'tr-proj':
                            currentProj = 'EPSG:2320'
                            $('a.dropdown-toggle[name="projection-dropdown"]').text(currentProj);
                            break;
                        case 'eu-proj':
                            currentProj = 'EPSG:4258'
                            $('a.dropdown-toggle[name="projection-dropdown"]').text(currentProj);
                            break;
                        case 'jp-proj':
                            currentProj = 'EPSG:6684'
                            $('a.dropdown-toggle[name="projection-dropdown"]').text(currentProj);
                            break;
                        case 'draw-city':
                            featureType = 'İl';
                            break;
                        case 'draw-county':
                            featureType = 'İlçe';
                            break;
                        case 'draw-neighborhood':
                            featureType = 'Mahalle';
                            break;
                        case 'draw-street':
                            featureType = 'Sokak';
                            break;
                        case 'draw-building':
                            featureType = 'Bina';
                            break;
                        case 'draw-transformer':
                            featureType = 'Trafo';
                            break;
                        case 'draw-straight':
                            featureType = 'Direk';
                            break;
                        case 'draw-subscriber':
                            featureType = 'Abone';
                            break;
                    }
                    //Tüm butonların etkinliğini geri aç
                    listButtons.prop('disabled', false);

                    currentType = featureType

                    $('a.dropdown-toggle[name="type-dropdown"]').text(currentType);
                });

                //INPUT ON CHANGE İLE GİRİLEN VERİNİN TİPİNİ KONTROL ET
                $('input.enter-coordinate[name="coordinate-entry-input"]', panel).on('change', function () {
                    const WKT = $('input.enter-coordinate[name="coordinate-entry-input"]').val(); // örnek bir WKT
                    const match = WKT.match(/^(\w+)\s*\(/); // WKT dizesinden geometri tipini alır
                    let geometryType = match[1]; // geometri tipini alır
                    geometryType = geometryType.charAt(0).toUpperCase() + geometryType.slice(1).toLowerCase(); // ilk harfi büyük yapar, diğer harfleri küçük yapar
                    console.log(geometryType); // "Polygon" çıktısını verir

                    hideTypeElement(geometryType, panel)
                })


                $('a.link[name="okey"]').on('click', function () {
                    if (currentProj == 'empty' || currentType == 'empty' || $('input.enter-coordinate[name="coordinate-entry-input"]').val() == '') {
                        Swal.fire({
                            icon: 'warning',
                            title: 'Lütfen boş alan bırakmadığınızdan emin olun!',
                            showConfirmButton: false,
                            timer: 1500
                        })
                    }
                    else {
                        try {
                            var wkt = $('input.enter-coordinate[name="coordinate-entry-input"]').val();
                            var format = new ol.format.WKT();
                            var feature = format.readFeature(wkt);
                            var geometry = feature.getGeometry();
                        } catch (error) {
                            Swal.fire({
                                icon: 'warning',
                                title: 'Hatalı WKT Girdiniz!',
                                showConfirmButton: false,
                                timer: 1500
                            })
                        }

                        if (geometry.getType() == 'Point') {
                            var coordinate = proj4(currentProj, 'EPSG:3857', geometry.getCoordinates());
                            feature.getGeometry().setCoordinates(coordinate);
                        }
                        else if (geometry.getType() == 'LineString') {
                            var array = [];
                            $.each(geometry.getCoordinates(), function (index) {
                                var coordinate = proj4(currentProj, 'EPSG:3857', geometry.getCoordinates()[index]);
                                array.push(coordinate);
                            })
                            feature.getGeometry().setCoordinates(array);
                        }
                        else if (geometry.getType() == 'Polygon') {
                            var arraysArray = [];
                            $.each(geometry.getCoordinates(), function (i) {
                                var array = [];
                                $.each(geometry.getCoordinates()[i], function (index) {
                                    var coordinate = proj4(currentProj, 'EPSG:3857', geometry.getCoordinates()[0][index]);
                                    array.push(coordinate);
                                })
                                arraysArray.push(array); //arraysArray[i].push(array);
                            })
                            feature.getGeometry().setCoordinates(arraysArray);
                        }

                        coordinate = format.writeFeature(feature);

                        source.addFeature(feature);
                        coordinatePanel.close();

                        //Kayıt paneli açılıyor
                        completeDrawing(feature, coordinate);
                    }
                })
            },
        },
    });
}

// ---> GENEL FONKSİYONLAR
function projectionInserter() {
    proj4.defs("EPSG:4326", "+proj=longlat +datum=WGS84 +no_defs +type=crs");
    proj4.defs("EPSG:3857", "+proj=merc +a=6378137 +b=6378137 +lat_ts=0 +lon_0=0 +x_0=0 +y_0=0 +k=1 +units=m +nadgrids=@null +wktext +no_defs +type=crs");
    proj4.defs("EPSG:4269", "+proj=longlat +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +no_defs +type=crs");
    proj4.defs("EPSG:2320", "+proj=tmerc +lat_0=0 +lon_0=30 +k=1 +x_0=500000 +y_0=0 +ellps=intl +towgs84=-84.1,-101.8,-129.7,0,0,0.468,1.05 +units=m +no_defs +type=crs");
    proj4.defs("EPSG:4258", "+proj=longlat +ellps=GRS80 +no_defs +type=crs");
    proj4.defs("EPSG:6684", "+proj=tmerc +lat_0=26 +lon_0=124 +k=0.9999 +x_0=0 +y_0=0 +ellps=GRS80 +towgs84=0,0,0,0,0,0,0 +units=m +no_defs +type=crs");
}
function deleteFeature(row, tableId, infoPanelItem) {
    var isDeleted = false;
    Swal.fire({
        title: 'Silmek istediğinize emin misiniz?',
        text: "Bu işlem geri alınamaz!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Evet, sil!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.get("/Request/DeleteLine",
                {
                    id: row.id,
                    featureType: row.featureType
                },
                function (records) {
                    if (records) {
                        $(tableId).bootstrapTable('remove', {
                            field: 'id',
                            values: [row.id]   //   <--- Sadece listeden siler Json dosyasında kalmaya devam eder
                        });
                        Swal.fire({
                            icon: 'success',
                            title: 'İçerik Başarıyla Silindi',
                            showConfirmButton: false,
                            timer: 1500
                        });
                        setTimeout(function () {
                            if (wmsSource) {
                                wmsSource.updateParams({ 'time': Date.now() });
                            }
                        }, 250);
                        if (infoPanelItem) {
                            infoPanelItem.close();
                        }
                        isDeleted = true;
                    } else {
                        Swal.fire({
                            position: 'top-end',
                            icon: 'error',
                            title: 'İçerik Silme İşlemi Başarısız',
                            showConfirmButton: false,
                            timer: 1500
                        })
                        isDeleted = false;
                    }
                });
        }
    })
    return isDeleted;
}
function fitMapLayer(row) {
    $.get("/Request/GetItemById", {
        id: row.id
    },
    function (records, status) {
        if (records) {
            var format = new ol.format.WKT();
            var feature = format.readFeature(records.coordinate);
            var event = feature;
            if (event.getGeometry().getType() == 'Point') {
                map.getView().fit(event.getGeometry().getExtent(), { maxZoom: 19 });
            }
            else {
                map.getView().fit(event.getGeometry().getExtent());
            }
        }
    })
}
function nullValueChecker(id1, id2, id3, id4, id5, id6, panel) {
    var contentArray = [];
    var isNull = true;
    
    var content1 = $(id1, panel).val();
    var content2 = $(id2, panel).val();
    var content3 = $(id3, panel).val();
    var content4 = $(id4, panel).val();
    var content5 = $(id5, panel).val();
    var content6 = $(id6, panel).val();

    if (id5 == '') {
        contentArray.push(content1, content2, content3, content4);
    }
    else if (id6 == '') {
        contentArray.push(content1, content2, content3, content4, content5);
    }
    else {
        contentArray.push(content1, content2, content3, content4, content5, content6);
    }

    console.log(contentArray);

    $.each(contentArray, function (index, content) {
        if (content == null || content == "" || content == undefined) {
            Swal.fire({
                icon: 'warning',
                title: 'Lütfen Boş Alan Bırakmayın!',
                showConfirmButton: false,
                timer: 1500
            })
            isNull = false;
            return isNull;
        }
    });
    return isNull;
}
function cancelGeometryChanges(feature) {
    if (infoModifyStatus == true) {
        infoModifyStatus = !($('button.modify-object[name="modify-button"]').data('modify-active'));
        $('button.modify-object[name="modify-button"]').data('modify-active', infoModifyStatus);
    }
    select.getFeatures().remove(feature);
    source.removeFeature(feature);
}

function hideTypeElement(geometryType, panel) {
    if (geometryType == 'Polygon') {
        $('a[name="street-brush"]', panel).addClass('disabled');
        $('a[name="straight-brush"]', panel).addClass('disabled');
        $('a[name="subscriber-brush"]', panel).addClass('disabled');
    }
    else if (geometryType == 'LineString') {
        $('a[name="city-brush"]', panel).addClass('disabled');
        $('a[name="county-brush"]', panel).addClass('disabled');
        $('a[name="neighborhood-brush"]', panel).addClass('disabled');
        $('a[name="building-brush"]', panel).addClass('disabled');
        $('a[name="transformer-brush"]', panel).addClass('disabled');
        $('a[name="straight-brush"]', panel).addClass('disabled');
        $('a[name="subscriber-brush"]', panel).addClass('disabled');
    }
    else if (geometryType == 'Point') {
        $('a[name="city-brush"]', panel).addClass('disabled');
        $('a[name="county-brush"]', panel).addClass('disabled');
        $('a[name="neighborhood-brush"]', panel).addClass('disabled');
        $('a[name="building-brush"]', panel).addClass('disabled');
        $('a[name="transformer-brush"]', panel).addClass('disabled');
        $('a[name="street-brush"]', panel).addClass('disabled');
    }

    var dropdownText;
    const buttons = $('a', panel);
    buttons.on('click', function (event) {
        //Tıklanan butonun işlemini çalıştır
        const action = $(event.currentTarget).data('action');
        switch (action) {
            case 'draw-city':
                dropdownText = 'İl';
                break;
            case 'draw-county':
                dropdownText = 'İlçe';
                break;
            case 'draw-neighborhood':
                dropdownText = 'Mahalle';
                break;
            case 'draw-street':
                dropdownText = 'Sokak';
                break;
            case 'draw-building':
                dropdownText = 'Bina';
                break;
            case 'draw-transformer':
                dropdownText = 'Trafo';
                break;
            case 'draw-straight':
                dropdownText = 'Direk';
                break;
            case 'draw-subscriber':
                dropdownText = 'Abone';
                break;
        }
        $('a.dropdown-toggle[name="draw-type"]').text(dropdownText)
    });
}
function setInputElement(records, featureType, panel) {
    switch (featureType) {
        case 'İl':
            $('input[name="license-plate"]', panel).val(records.licensePlate);
            $('input[name="population"]', panel).val(records.population);
            break;
        case 'İlçe':
            $('input[name="population"]', panel).val(records.population);
            break;
        case 'Mahalle':
            $('input[name="located-county"]', panel).val(records.locatedCounty);
            $('input[name="population"]', panel).val(records.population);
            break;
        case 'Bina':
            $('input[name="building-no"]', panel).val(records.buildingNo);
            $('input[name="floor-num"]', panel).val(records.floorNum);
            break;
        case 'Trafo':
            $('input[name="located-county"]', panel).val(records.locatedCounty);
            $('input[name="located-neighborhood"]', panel).val(records.locatedNeighborhood);
            $('input[name="capacity"]', panel).val(records.capacity);
            break;
        case 'Sokak':
            $('input[name="located-county"]', panel).val(records.locatedCounty);
            $('input[name="located-neighborhood"]', panel).val(records.locatedNeighborhood);
            break;
        case 'Direk':
            $('input[name="straight-type"]', panel).val(records.straightType);
            break;
        case 'Abone':
            $('input[name="located-county"]', panel).val(records.locatedCounty);
            $('input[name="located-neighborhood"]', panel).val(records.locatedNeighborhood);
            break;
    }
}

function post(panel, dataEntryPanel, featureType, event, coordinate) {
    var inputNum = $('input[name="num"]', panel).val();  //INT
    var inputStr = $('input[name="str"]', panel).val();  //STRING    
    var locatedCity = $('input[name="located-city"]', panel).val();   //STRING
    var locatedCounty = $('input[name="located-county"]', panel).val(); //STRING
    var locatedNeighborhood = $('input[name="located-neighborhood"]', panel).val(); //STRING
    var licensePlate = $('input[name="license-plate"]', panel).val();  //INT
    var population = $('input[name="population"]', panel).val();    //INT
    var straightType = $('input[name="straight-type"]', panel).val();  //STRING
    var capacity = $('input[name="capacity"]', panel).val();      //STRING
    var buildingNo = $('input[name="building-no"]', panel).val();    //INT
    var floorNum = $('input[name="floor-num"]', panel).val();  //INT

    switch (featureType) {
        case 'İl':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="license-plate"]', 'input[name="population"]', "", panel)) {
                $.post("/Request/PostCity",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        licenseplate: licensePlate,
                        population: population
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        case 'İlçe':
            //if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="population"]', "", "", panel)) {
                $.post("/Request/PostCounty",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        population: population
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            //}
            break;
        case 'Mahalle':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="population"]', "", panel)) {
                $.post("/Request/PostNeighborhood",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        population: population
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        case 'Bina':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="building-no"]', 'input[name="floor-num"]', "", panel)) {
                $.post("/Request/PostBuilding",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        buildingno: buildingNo,
                        floornum: floorNum
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        case 'Trafo':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="capacity"]', 'input[name="located-county"]', 'input[name="located-neighborhood"]', panel)) {
                $.post("/Request/PostTransformer",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        capacity: capacity,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        locatedneighborhood: locatedNeighborhood
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        case 'Sokak':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="located-neighborhood"]', "", panel)) {
                $.post("/Request/PostStreet",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        locatedneighborhood: locatedNeighborhood
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        case 'Direk':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="straight-type"]', "", "", panel)) {
                $.post("/Request/PostStraight",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedCity: locatedCity,
                        straightType: straightType,
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        case 'Abone':
            if (nullValueChecker('input[name="num"]', 'input[name="str"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="located-neighborhood"]', "", panel)) {
                $.post("/Request/PostSubscriber",
                    {
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        locatedneighborhood: locatedNeighborhood
                    },
                    function (records) {
                        postRecords(records)
                    }
                )
            }
            break;
        default:
            console.log("İçerik tipi belirlenemedi!");
            break;
    }

    function postRecords(records) {
        //Girilen verinin tabloya anlık olarak eklenmesi sağlanıyor
        if (records) {
            $('#Records-Table').bootstrapTable('insertRow', {
                index: 0,
                row: {
                    id: 'NEW',
                    name: inputStr,
                    number: inputNum,
                    locatedCity: locatedCity,
                    featureType: featureType
                }
            });
            dataEntryPanel.close();
            Swal.fire({
                position: 'top-end',
                icon: 'success',
                title: 'İçerik Kaydedildi',
                showConfirmButton: false,
                timer: 1500
            })
            setTimeout(function () {
                if (wmsSource) {
                    wmsSource.updateParams({ 'time': Date.now() });
                }
                source.removeFeature(event);
            }, 250);    
        } else {
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: 'İçerik Kaydedilemedi!',
                text: 'Sunucu tarafından uyumsuz bir istek gönderildi.',
                showConfirmButton: false,
                timer: 1500
            })
        }
    }
}
function put(panel, dataQueryPanel, records, event, index, recordsTable) {
    var inputNum = $('input[name="street-number"]', panel).val();  //INT
    var inputStr = $('input[name="street-name"]', panel).val();  //STRING    
    var locatedCity = $('input[name="located-city"]', panel).val();   //STRING
    var locatedCounty = $('input[name="located-county"]', panel).val(); //STRING
    var locatedNeighborhood = $('input[name="located-neighborhood"]', panel).val(); //STRING
    var licensePlate = $('input[name="license-plate"]', panel).val();  //INT
    var population = $('input[name="population"]', panel).val();    //INT
    var straightType = $('input[name="straight-type"]', panel).val();  //STRING
    var capacity = $('input[name="capacity"]', panel).val();      //STRING
    var buildingNo = $('input[name="building-no"]', panel).val();    //INT
    var floorNum = $('input[name="floor-num"]', panel).val();  //INT
    var featureType = $('a.nav-link[name="draw-type"]', panel).text();  //STRING
    var coordinate = $('textarea.form-control[name="coordinate"]', panel).val();   //STRING

    switch (records.featureType) {
        case 'İl':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="license-plate"]', 'input[name="population"]', "", panel)) {
                $.post("/Request/PutCity",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        licenseplate: licensePlate,
                        population: population
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'İlçe':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="population"]', "", "", panel)) {
                $.post("/Request/PutCounty",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        population: population
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'Mahalle':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="population"]', "", panel)) {
                $.post("/Request/PutNeighborhood",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        population: population
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'Bina':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="building-no"]', 'input[name="floor-num"]', "", panel)) {
                $.post("/Request/PutBuilding",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        buildingno: buildingNo,
                        floornum: floorNum
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'Trafo':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="capacity"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="located-neighborhood"]', panel)) {
                $.post("/Request/PutTransformer",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        capacity: capacity,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        locatedneighborhood: locatedNeighborhood
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'Sokak':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="located-neighborhood"]', "", panel)) {
                $.post("/Request/PutStreet",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        locatedneighborhood: locatedNeighborhood
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'Direk':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="straight-type"]', "", "", panel)) {
                $.post("/Request/PutStraight",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: featureType,
                        coordinate: coordinate,
                        locatedCity: locatedCity,
                        straightType: straightType,
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        case 'Abone':
            if (nullValueChecker('input[name="street-number"]', 'input[name="street-name"]', 'input[name="located-city"]', 'input[name="located-county"]', 'input[name="located-neighborhood"]', "", panel)) {
                $.post("/Request/PutSubscriber",
                    {
                        id: records.id,
                        number: inputNum,
                        name: inputStr,
                        measurement: calculateMeasurement(event),
                        featureType: records.featureType,
                        coordinate: coordinate,
                        locatedcity: locatedCity,
                        locatedcounty: locatedCounty,
                        locatedneighborhood: locatedNeighborhood
                    },
                    function (records) {
                        putRecords(records)
                    }
                )
            }
            break;
        default:
            console.log("İçerik tipi bulunamadı!")
            break;
    }

    function putRecords(records) {
        //Girilen verinin tabloya anlık olarak eklenmesi sağlanıyor
        if (records) {
            $(recordsTable).bootstrapTable('updateRow', {
                index: index,
                row: {  //   <--- Sadece listede günceller Json dosyasında bir işlem yapmaz
                    name: inputStr,
                    number: inputNum,
                    locatedCity: locatedCity
                    //featureType: records.featureType
                }
            });
            console.log("Tablo Güncellendi!");

            dataQueryPanel.close()
            Swal.fire(
                'Güncellendi!',
                'Veriniz güncellendi.',
                'success'
            )

            setTimeout(function () {
                if (wmsSource) {
                    wmsSource.updateParams({ 'time': Date.now() });
                }
                source.removeFeature(event);
            }, 250); 

        } else {
            Swal.fire({
                position: 'top-end',
                icon: 'error',
                title: 'İçerik Güncellenemedi!',
                text: 'Sunucu tarafından uyumsuz bir istek gönderildi.',
                showConfirmButton: false,
                timer: 1500
            })
        }
    }
}

function requestString(featureType, beforeStr, afterStr) {
    var str;
    switch (featureType) {
        case 'İl':
            str = "City"
            break;
        case 'İlçe':
            str = "County"
            break;
        case 'Mahalle':
            str = "Neighborhood"
            break;
        case 'Bina':
            str = "Building"
            break;
        case 'Trafo':
            str = "Transformer"
            break;
        case 'Sokak':
            str = "Street"
            break;
        case 'Direk':
            str = "Straight"
            break;
        case 'Abone':
            str = "Subscriber"
            break;
    }
    return beforeStr + str + afterStr;
}

function multiSelectedFeatures() {
    const selectedFeatures = select.getFeatures();
    var featureValues;
    var html;

    // clear selection when drawing a new box and when clicking on the map
    dragBox.on('boxstart', function () {
        selectedFeatures.clear();
    });

    dragBox.on('boxend', function () {
        dragBox.setActive(false);
        const extent = dragBox.getGeometry().getExtent();
        const kmlLayer = map.getLayers().getArray().find(layer => layer instanceof ol.layer.Vector && layer.get('title') === 'loaded-kml-file');
        const boxFeatures = kmlLayer.getSource().getFeaturesInExtent(extent).filter((feature) => feature.getGeometry().intersectsExtent(extent));

        selectedFeatures.extend(boxFeatures);

        var tableValues = [];

        var tableValues = selectedFeatures.array_.map(function (feature) {
            featureValues = feature.values_;
            html = featureValues.description;

            const parser = new DOMParser();
            const doc = parser.parseFromString(html, "text/html");

            const liElements = doc.querySelectorAll('.textattributes li');
            let tuikCode = 0;
            let cityName = '';
            let plateNo = 0;

            liElements.forEach((liElement) => {
                const nameElement = liElement.querySelector('.atr-name');
                const valueElement = liElement.querySelector('.atr-value');
                if (nameElement && valueElement) {
                    switch (nameElement.textContent.trim()) {
                        case 'TUIK_IL_KODU':
                            tuikCode = valueElement.textContent.trim();
                            break;
                        case 'ILADI':
                            cityName = valueElement.textContent.trim();
                            break;
                        case 'PLAKANO':
                            plateNo = valueElement.textContent.trim();
                            break;
                    }
                }
            });

            return {
                tuikCode: tuikCode,
                cityName: cityName,
                plateNo: plateNo
            };
        });

        jsPanel.create({
            position: "center",
            contentSize: "auto",
            headerTitle: "Test Listesi",
            theme: "dark",
            id: 'multi-select-panel',
            //clickToSelect: true,
            checkbox: true,
            borderRadius: '8px',
            content: '<table id="multi-select-table"></table>',

            callback: function (panel) {
                $('#multi-select-table', panel).bootstrapTable({
                    data: tableValues,
                    height: 'auto',
                    search: true,
                    pagination: true,
                    columns: [{
                        title: 'İl',
                        field: 'cityName',
                        sortable: true
                    }, {
                        title: 'Plaka Kodu',
                        field: 'plateNo',
                        sortable: true
                    }, {
                        title: 'TÜİK Kodu',
                        field: 'tuikCode',
                        sortable: true
                    }, {
                        title: 'İşlevler',
                        width: '150px',
                        formatter: function (value, row, index) {
                            return '<div class="btn-group" role="group" aria-label="İşlevler"><button type="button" class="btn btn-success save-row" title="Kaydet" aria-label="Kaydet"><i class="fa fa-check" aria-hidden="true"></i></button><button type="button" class="btn btn-warning view-object" title="Haritada göster" aria-label="Haritada göster"><i class="fa fa-eye" aria-hidden="true"></i></button></div>';
                        },
                        events: {
                            'click .save-row': function (e, value, row, index) {
                                $.each(selectedFeatures.array_, function (i, k) {
                                    if (row.cityName === k.values_.name) {
                                        var WKTFormat = new ol.format.WKT();
                                        var coordinateValue = WKTFormat.writeFeature(k)
                                        featureType = 'İl';
                                        completeDrawing(k, coordinateValue);
                                    }
                                });
                            },
                            'click .view-object': function (e, value, row, index) {
                                $.each(selectedFeatures.array_, function (i, k) {
                                    if (row.cityName === k.values_.name) {
                                        if (k.getGeometry().getType() == 'Point') {
                                            map.getView().fit(k.getGeometry().getExtent(), { maxZoom: 19 });
                                        }
                                        else {
                                            map.getView().fit(k.getGeometry().getExtent());
                                        }
                                    }
                                });

                            }
                        }
                    }],
                })
            }
        })
    });
}

function keyTimeWarning() {
    var sessionDuration = new Date(admin.sessionDuration);

    // Belirli aralıklarla sürenin dolduğunu kontrol etmek için setInterval kullanılıyor
    var interval = setInterval(function () {
        var remainingTime = Math.floor((sessionDuration - Date.now()) / 1000 / 60);

        if (remainingTime < 30) {
            console.log("Süre Doluyor!");
            clearInterval(interval); // Süre dolunca interval'ı durdur
            showRenewTokenButton(); // Tokeni yenileme butonunu göster
        }
    }, 60000); // 1 dakikalık aralıklarla kontrol yap

    function showRenewTokenButton() {
        // Tokeni yenileme butonunu oluştur
        var renewTokenButton = $('<button id="refresh-token-button" class="btn btn-success" title="Oturumu Yenile" aria-label="Oturumu Yenile"><i class="fa-solid fa-unlock-keyhole" aria-hidden="true"></i> Oturumu Yenile </button>');

        // Butona tıklanınca yapılacak işlemi tanımla
        renewTokenButton.on('click', function () {
            // Tokeni yenileme işlemlerini burada gerçekleştir
            console.log('Token yenileniyor...');
            $.get('/Request/RefreshToken', function (result, status) {
                if (status == 'success') {
                    if (result != admin.sessionDuration) {
                        admin.sessionDuration = result;
                        renewTokenButton.remove();
                        keyTimeWarning();
                    }
                }
            });
        });

        // Butonu sayfaya ekle (örneğin body elementine)
        $('body').append(renewTokenButton);
    }
}