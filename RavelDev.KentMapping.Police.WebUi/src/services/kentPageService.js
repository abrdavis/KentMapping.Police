import axios from "axios";


export const kentPageService = {
    getHomePageModel: getHomePageModel,
};

function getHomePageModel() { 
    return axios.get(`${import.meta.env.VITE_API_URL}/PoliceIncident/GetHomePageModel`, { crossDomain: true })
        .then(response => {
            return response;
        });
}

