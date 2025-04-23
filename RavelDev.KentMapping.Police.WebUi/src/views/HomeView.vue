<script setup>
import "leaflet/dist/leaflet.css"
import * as L from 'leaflet';
import moment from 'moment';


import { toRaw, ref, onMounted, shallowRef } from 'vue'
import { kentPageService, kentPoliceService } from '@/services';
import icon from "leaflet/dist/images/marker-icon.png";
import iconShadow from "leaflet/dist/images/marker-shadow.png";

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

async function queryData() {
    const startDateParam = moment(startDate.value).format('YYYY-MM-DD');
    const endDateParam =  moment(endDate.value).format('YYYY-MM-DD');
    const res = await kentPoliceService.getIncidentsForTypeAndDate(selectedIncident.value, startDateParam, endDateParam);
    console.log(res)
    if (res.data) {
        clearMarkers();
        res.data.data.forEach((incident) => {

            let unproxiedMap = toRaw(policeMap.value);
            let marker = L.marker([incident.longitude, incident.latitude], {icon: DefaultIcon});
            marker.addTo(unproxiedMap)
                .bindPopup(`${incident.incidentDescription}<br/> ${incident.caseReportDescription} <br/> ${incident.dateDisplayString}`);
            activeMapMarkers.push(marker);
        })
    }
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
    <div class="row mb-5">

        <div class="col-4">
            <v-select v-model="selectedIncident" label="Criminal Infraction" :items="incidentTypes"
                item-title="typeName" item-value="policeIncidentTypeId"></v-select>
        </div>
        <div class="col-3">
            <VueDatePicker auto-apply :format="dateFormat" placeholder="Start Date" v-model="startDate"></VueDatePicker>
            <VueDatePicker auto-apply :format="dateFormat" placeholder="End Date" v-model="endDate"></VueDatePicker>
        </div>
        <div class="col-3">
            <v-btn variant="outlined" @click="queryData">
                Lookup Police Stats
            </v-btn>
        </div>
    </div>
    <div>
        <div id="map" style="height:90vh;"></div>
    </div>
</template>


<style>
html,
body {
    margin: 0;
    padding: 0;
}
</style>