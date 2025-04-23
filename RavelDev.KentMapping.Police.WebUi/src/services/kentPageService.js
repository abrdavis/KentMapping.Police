import axios from "axios";

const baseUrl = `https://localhost:7047/api/PoliceIncident`;

export const kentPageService = {
    getHomePageModel: getHomePageModel,
};

function getHomePageModel() { 
    return axios.get(`${baseUrl}/GetHomePageModel`, { crossDomain: true })
        .then(response => {
            return response;
        });
}

