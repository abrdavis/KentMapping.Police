<script setup>
import "leaflet/dist/leaflet.css"
import * as L from 'leaflet';
import moment from 'moment';
import "leaflet.heat"


import { toRaw, ref, onMounted, watch } from 'vue'
import { kentPageService, kentPoliceService } from '@/services';
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";
import { toast } from 'vue3-toastify';
import "vue3-toastify/dist/index.css";

let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconSize: [24, 36],
    iconAnchor: [12, 36],
});

const policeMap = ref(null)
const maxDate = ref(null)
const minDate = ref(null)
const endDate = ref(null)
const startDate = ref(null)

const incidentTypes = ref([])
const zoom = ref(15)
const center = ref([41.15385614546634, -81.34433118919121])
const dateFormat = "M/dd/yyyy";
const selectedIncident = ref(null)
let activeMapMarkers = []
let activePoints = [];
let heatLayer = null;
let incidentsToDisplay = [];


const displayTypes = [
    {
        displayName: "Markers",
        id: 1
    },
    {
        displayName: "Heat Map",
        id: 2
    }

]

const selectedDisplayType = ref(1);
watch(selectedDisplayType, (newValue, oldValue) => {
    updateMap();
});

kentPageService.getHomePageModel().then(res => {
    if (res.data && res.data.success) {
        incidentTypes.value = res.data.model.incidentDescriptions;
        startDate.value = minDate.value = res.data.model.minDate;
        endDate.value = maxDate.value = res.data.model.maxDate;
    }
})


function clearMarkers() {
    let unproxiedMap = toRaw(policeMap.value);
    for (var i = 0; i < activeMapMarkers.length; i++) {
        unproxiedMap.removeLayer(activeMapMarkers[i]);
    }
    activeMapMarkers = [];
}

function clearHeatLayer() {
    if (heatLayer)
        heatLayer.setLatLngs([]);
}

function updateMap() {
    if (!incidentsToDisplay || incidentsToDisplay.length == 0) return;
    let unproxiedMap = toRaw(policeMap.value);
    if (selectedDisplayType.value == 1) {
        clearHeatLayer();
        incidentsToDisplay.forEach((incident) => {
            let marker = L.marker([incident.longitude, incident.latitude], { icon: DefaultIcon });
            marker.addTo(unproxiedMap)
                .bindPopup(`${incident.incidentDescription}<br/> ${incident.caseReportDescription} <br/> ${incident.dateDisplayString}`);
            activeMapMarkers.push(marker);
        })
    }
    else if (selectedDisplayType.value == 2) {
        clearMarkers();
        if (!heatLayer)
            heatLayer = L.heatLayer(activePoints, { max: .5, radius: 17 }).addTo(unproxiedMap);
        else
            heatLayer.setLatLngs(activePoints)
    }



}

async function queryData() {

    if (!selectedIncident.value) {
        toast("Please select a criminal infraction.", {
            "theme": "dark",
            "type": "error",

        });
        return;
    }
    const startDateParam = moment(startDate.value).format('YYYY-MM-DD');
    const endDateParam = moment(endDate.value).format('YYYY-MM-DD');
    const res = await kentPoliceService.getIncidentsForTypeAndDate(selectedIncident.value, startDateParam, endDateParam);
    console.log(res)
    if (res.data) {
        clearMarkers();
        clearHeatLayer();
        activePoints = [];
        activeMapMarkers = [];
        incidentsToDisplay = res.data.incidentData;
        incidentsToDisplay.forEach((incident) => {
            let point = [incident.longitude, incident.latitude];
            activePoints.push(point)
        })
        updateMap()
    }
}

function onDisplayTypeSelect(e) {
    console.log(selectedDisplayType.value)
}

onMounted(() => {
    policeMap.value = L.map('map').setView(center.value, zoom.value);
    let unproxiedMap = toRaw(policeMap.value);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="http://www.openstreetmap.org/copyright">OpenStreetMap</a>'
    }).addTo(unproxiedMap);


});
</script>

<template>
    <main>
        <div class="row mb-5">
            <div class="col-12">
                <div id="map"></div>
            </div>
        </div>
        <div class="row">
            <div class="col-lg-6 col-12">
                <v-select v-model="selectedIncident" label="Criminal Infraction" :items="incidentTypes"
                    item-title="typeName" item-value="policeIncidentTypeId"></v-select>

            </div>
            <div class="col-lg-6 col-12">
                <v-select v-model="selectedDisplayType" label="Display Type" :items="displayTypes"
                    item-title="displayName" item-value="id" @change="onDisplayTypeSelect"></v-select>
            </div>
        </div>
        <div class="row mb-2">
            <div class="col-6">
                    <VueDatePicker auto-apply :format="dateFormat" placeholder="Start Date" v-model="startDate">
                    </VueDatePicker>
            </div>
            <div class="col-6">
                <VueDatePicker auto-apply :format="dateFormat" placeholder="End Date" v-model="endDate">
                </VueDatePicker>
            </div>
        </div>
        <div class="row">
            <div class="col-12">
                <div class="form-group">
                    <v-btn variant="outlined" @click="queryData" id="lookupBtn">
                        Lookup Stats
                    </v-btn>

                </div>
            </div>
        </div>
    </main>

</template>


<style>
@media only screen and (max-width: 960px) {
    #map {
        height: 54vh !important;
    }

}

.container {
    height: 100vh;
}

#map {
    height: 82vh;
}

#lookupBtn {
    width: 100%;
}

html,
body {
    margin: 0;
    padding: 0;
}
</style>