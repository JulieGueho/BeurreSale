(function ($) {
    var map;
    var resultLayer;

    $(function () {

        // Define the map to use from MapBox
        // This is the TileJSON endpoint copied from the embed button on your map
        var url = 'http://a.tiles.mapbox.com/v3/juliegueho.map-vk2o7c8o.jsonp';

        // Make a new Leaflet map in your container div
        map = new L.Map('map_canvas') // container's id="mapbox"

        // Center the map on Paris, at zoom 13
		.setView(new L.LatLng(48.863, 2.324), 15);

        // Get metadata about the map from MapBox
        wax.tilejson(url, function (tilejson) {
            map.addLayer(new wax.leaf.connector(tilejson));
        });

        setMarkerLayer();

        $("#search-submit").submit(function () {
            searchSubmit(this);
            return false;
        });

        //map.on('moveend', setMarkerLayer);

        // Success notifications displays when info ok is sent from previous page
        if ($("input[name='success']").length > 0) {
            $("#notification-bar").show().delay(4000).fadeOut('slow'); //Notification disappears itself after 4s
            $("#notification-bar .close").click(function () { // Or it can be closed manually
                $("#notification-bar").fadeOut('slow');
            });
        }
    });

    // Set markers from db on the map
    function setMarkerLayer() {
        var group = new L.LayerGroup();

        $.getJSON('SaltedButter', function (data) {
            $.each(data, function (index, qPlace) {
                var place = mapQPlaceToPlace(qPlace);
                var marker = addMarker(place);
                group.addLayer(marker);
            });
            map.addLayer(group);
        });
    }

    // Add the marker matching the given place on the map
    function addMarker(place) {
        // Propriété du marker
        var latlng = new L.LatLng(place.Latitude, place.Longitude);

        var image = '';
        switch (place.Type) {
            case 'salted':
                image = 'static/img/pinvert.png';
                break;
            case 'unsalted':
                image = 'static/img/pinrouge.png';
                break;
            default:
                image = 'static/img/marker.png';
                break;
        }

        var placeIconType = L.Icon.extend({ options: {
            iconUrl: image,
            shadowUrl: null,
            iconSize: new L.Point(28, 40),
            shadowSize: null,
            iconAnchor: new L.Point(14, 40),
            popupAnchor: new L.Point(0, -40)
        }
        });

        var placeIcon = new placeIconType();

        var zindex = 1;
        // If result of API, marker appears above
        if (place.Type == 'result') {
            zindex = 1000;
        }

        var marker = new L.Marker(latlng, { icon: placeIcon, zIndexOffset: zindex });

        marker._leaflet_id = "id" + place.ID; // Add "id" so there is no confusion with another leaflet object id

        var markerContent = "<div class='adressInfoWindow'><i class='icon-map-marker'></i> " + place.Name + "<br>" + place.Vicinity + "</div>"

        if (place.Type == 'result') {
            markerContent += "<br>Nous ne connaissons pas cet endroit<br><div id='" + place.ID + "' class='addAddressLink'>Ajouter cette adresse ?</div>";
            marker.bindPopup(markerContent).on('click', onPlaceAddClick);
        }
        else {
            marker.bindPopup(markerContent).on('click', onMarkerClick);
        }

        return marker;
    }

    function onPlaceAddClick(e) {
        $(".leaflet-popup-content-wrapper").on('click', function (e) {
            var placeId = $(e.target).attr('id');

            $.get("SaltedButter/NoteAdd?id=" + placeId, function (data) {
                setAddNote(data);
                $("body").css('cursor', 'auto');
                $("#note").show();
            });

            $("body").css('cursor', 'wait');
        });
    }


    function onMarkerClick(e) {

        var id = e.target._leaflet_id.substring(2);

        $.get("SaltedButter/Note?id=" + id, function (data) {
            $("#note").html(data);
            setupCloseBehavior();
        });
    }

    function onPlaceSubmit(e) {

        var data = $("#addForm").serialize();

        $.ajax({
            type: "POST",
            url: "SaltedButter/Create",
            data: data,
            success: function (data) {
                if (data.url) {
                    window.location.href = data.url;
                }
                else {
                    setAddNote(data);
                }
            }
        });
        return false;
    }

    function setAddNote(data) {
        $("#note").html(data);
        $("#addForm").submit(function () {
            onPlaceSubmit(this);
            return false;
        });

        setupCloseBehavior();
    }

    //When closing marker popup, note closes as well
    //When closing note, marker popup closes as well
    function setupCloseBehavior() {
        $("#note").show();
        $(".close,.leaflet-popup-close-button").click(function () {
            $("#note").fadeOut('slow');
            map.closePopup();
        })

    }

    function searchSubmit(e) {
        // URL of Nokia API RESTful Places
        // REQUEST must be replaced by request entered by user
        var url = "http://demo.places.nlp.nokia.com/places/v1/discover/search?at=48.856,2.352&q=REQUEST&size=10&app_id=EOXbMEWYAllPhQnAQsmn&app_code=9TIppnJDB9PHy8-ckJLWXA";

        // No spaces, no accents, no upper case
        var request = noaccent($("#appendedInputButton").val().trim().toLowerCase());

        // Replacing parameters in the request
        var urlrequest = url.replace("REQUEST", request);

        // Getting the last word of the request and passing it as the city
        // (Case won't be considered if it's not a city, see below)
        var listWord = request.split(" ");
        var city;
        if (listWord.length > 1) {
            city = listWord[listWord.length - 1];
        }

        // Requesting API
        $.getJSON(urlrequest, function (data) {

            // Initializing var
            var exactCityList = [];
            var exactStreetList = [];
            var exactPlaceList = [];
            var exactMatch = false;
            var localPlaceList = [];
            var otherPlaceList = [];

            // Running throught the result to classified them
            $.each(data.results.items, function (index, result) {

                // Looking for article between parenthesis which must go at the front ex: Mouton Noir (Le)
                var patt = new RegExp("\\([a-z]*\\)", "i");
                var name = result.title;
                if (patt.test(name)) // If RegEx matches
                {
                    var occurence = patt.exec(name, ""); // Getting in-between parenthesis expression
                    name = name.replace(occurence[0], "").trim(); // Removing in-between parenthesis expression from the end of the name
                    var article = occurence[0].replace(/\(/g, "").replace(/\)/g, ""); // Getting expression alone
                    result.title = article + " " + name; // Putting expression at the beginning
                }

                // Filtering out results out of France
                if (result.vicinity.indexOf("France") != -1) {
                    // Looking for an exact match
                    if (noaccent(result.title.toLowerCase()) == request || noaccent(result.title.toLowerCase().replace("-", " ")) == request) {
                        // Taking only towns, no lieux-dits
                        if (result.category.id == "city-town-village" && result.vicinity == "France") {
                            exactCityList.push(result);
                        }
                        else if (result.category.id == "street-square" || result.category.id == "building") {
                            exactStreetList.push(result);
                        }
                        else if (result.category.id != "city-town-village") {
                            exactPlaceList.push(result);
                        }

                        exactMatch = true;
                    }
                    else {
                        // If no exact match is found, checking if the result belongs to the specified city
                        if (city != null) {
                            if (result.vicinity.toLowerCase().indexOf(city) != -1 && allWordMatch(result.title, request)) {
                                localPlaceList.push(result);
                            }
                        }
                        else {
                            otherPlaceList.push(result);
                        }
                    }
                }
            });

            if (exactMatch) {
                if (exactCityList.length > 0) {
                    display(exactCityList);
                }
                else if (exactStreetList.length > 0) {
                    display(exactStreetList);
                }
                else if (exactPlaceList.length > 0) {
                    display(exactPlaceList);
                }
            }
            else if (localPlaceList.length > 0) {
                display(localPlaceList);
            }
            else if (otherPlaceList.length > 0) {
                display(otherPlaceList);
            }
            $('body').css('cursor', 'auto');
        });

        $('#note').hide();
        map.closePopup();
        $('body').css('cursor', 'wait');
    }

    function display(resultList) {
        if (resultLayer != null && map.hasLayer(resultLayer)) {
            map.removeLayer(resultLayer);
        }
        resultLayer = new L.LayerGroup();

        var containerSouthWest = map.layerPointToLatLng(new L.Point($("#search-header").height(), screen.width / 3));
        var containerNorthEast = map.layerPointToLatLng(new L.Point($("#search-header").height() + $("#header").height(), screen.width / 3));
        var mapBounds = map.getBounds();

        var difXSouthWest = containerSouthWest.lng - mapBounds.getSouthWest().lng;
        var difYNorthEast = containerNorthEast.lat - mapBounds.getNorthEast().lat;

        // If there is one result only then it is displayed and the map viewport fits the marker
        if (resultList.length == 1) {
            place = mapNokiaResultToPlace(resultList[0]);

            // If viewport bounds are specified (ex city)
            if (resultList[0].bbox != null) {
                var southWest = new L.LatLng(resultList[0].bbox[1], resultList[0].bbox[0]),
				northEast = new L.LatLng(resultList[0].bbox[3], resultList[0].bbox[2]),
				bounds = new L.LatLngBounds(southWest, northEast);
                map.fitBounds(bounds);
                map.zoomIn(); // ZoomIn to fit tighter than the default zoom

            }
            else {
                // Zoom will be the same than previous one
                map.panTo(new L.LatLng(place.Latitude, place.Longitude));
            }

            var marker = addMarker(place);
            resultLayer.addLayer(marker);
            marker.openPopup();
        }
        else {
            var maxLat;
            var maxLng;
            var minLat;
            var minLng;
            var place;
            var nbMarkerVisible = 0;
            var marker;
            $.each(resultList, function (index, result) {
                place = mapNokiaResultToPlace(result);

                // Place is added whatever the bounds are to avoid making another loop
                marker = addMarker(place);
                resultLayer.addLayer(marker);

                if (map.getBounds().contains(new L.LatLng(place.Latitude, place.Longitude))) {
                    // If there's at least one marker visible then the viewport remains still
                    nbMarkerVisible++;
                }
                else {
                    // If the marker is not visible, it's tested to set viewport bounds
                    if (maxLng < place.Longitude || maxLng == null) {
                        maxLng = place.Longitude;
                    }
                    if (minLng > place.Longitude || minLng == null) {
                        minLng = place.Longitude;
                    }
                    if (maxLat < place.Latitude || maxLat == null) {
                        maxLat = place.Latitude;
                    }
                    if (minLat > place.Latitude || minLat == null) {
                        minLat = place.Latitude;
                    }
                }
            });

            // If no marker is visible then the map is set to fit all markers
            if (nbMarkerVisible == 0) {
                var southWest = new L.LatLng(minLat, minLng),
				northEast = new L.LatLng(maxLat, maxLng),
				bounds = new L.LatLngBounds(southWest, northEast);
                map.fitBounds(bounds);
                map.zoomIn(); // Because default zoom is not close enough
            }
        }

        map.addLayer(resultLayer);
    }

    function noaccent(chaine) {
        temp = chaine.replace(/[\xE0-\xE6]/gi, "a")
        temp = temp.replace(/[\xE8-\xEB]/gi, "e")
        temp = temp.replace(/[\xEC-\xEF]/gi, "i")
        temp = temp.replace(/[\xF2-\xF6]/gi, "o")
        temp = temp.replace(/[\xF9-\xFC]/gi, "u")
        return temp
    }

    function allWordMatch(strToTest, strBase) {
        var returnValue = true;
        strToTest = noaccent(strToTest.toLowerCase()).replace("de ", " ").replace("du ", " ").replace("des ", " ").replace("de la ", " ").replace("d\'", "").replace("\'", " ").replace("le ", " ").replace("la ", " ").replace("les ", " ").replace("-", " ");
        strBase = noaccent(strBase.toLowerCase());
        // Running through words of the string to test
        $.each(strToTest.split(" "), function (index, word) {
            // If a single word of string to test is not in the base string then the result is false
            if (strBase.indexOf(word) == -1) {
                returnValue = false;
            }
        });

        return returnValue;
    }

    function mapQPlaceToPlace(qPlace) {
        var vicinity = '';
        if (qPlace.Place.Address1 !== null) {
            vicinity += qPlace.Place.Address1 + "<br>";
        }

        if (qPlace.Place.Address2 !== null) {
            vicinity += qPlace.Place.Address2 + "<br>";
        }

        if (qPlace.Place.PostalCode !== null || qPlace.Place.City !== null) {
            vicinity += qPlace.Place.PostalCode + " " + qPlace.Place.City + "<br>";
        }

        place = {
            'ID': qPlace.ID,
            'Latitude': qPlace.Place.Latitude,
            'Longitude': qPlace.Place.Longitude,
            'Name': qPlace.Place.Name,
            'Type': qPlace.SaltedButter.Salted ? 'salted' : 'unsalted',
            'Vicinity': vicinity,
            'Username': qPlace.User.Username,
            'CreationDate': qPlace.CreationDate
        };

        return place;
    }

    function mapNokiaResultToPlace(nokiaPlace) {
        place = {
            'ID': nokiaPlace.id,
            'Latitude': nokiaPlace.position[0],
            'Longitude': nokiaPlace.position[1],
            'Name': nokiaPlace.title,
            'Type': 'result',
            'Vicinity': nokiaPlace.vicinity
        };
        return place;
    }

    function mapNokiaPlaceToPlace(nokiaPlace) {
        var name = '';
        if (nokiaPlace.categories[0] != "building") {
            name = nokiaPlace.name;
        }

        place = {
            'ID': nokiaPlace.placeId,
            'Latitude': nokiaPlace.location.position[0],
            'Longitude': nokiaPlace.location.position[1],
            'Name': name,
            'Type': 'result',
            'Vicinity': nokiaPlace.location.address.text
        };
        return place;
    }

})(jQuery);