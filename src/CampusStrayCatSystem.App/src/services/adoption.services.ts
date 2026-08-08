import {http} from './http'
import {toCat} from './cats.service'
export const adoptionService={
    async getOnCampusCats(){
        const {data}=await http.get('/cat',{params:{lifeStatus:'ON_CAMPUS'}})
        return data.map(toCat)
    },
    async postAdoption(catID:string,applicationUserID:string){
        await http.post('/adoption',{params:{CatID:catID,applicationUserID:applicationUserID,status:'PENDING'}})
    }
}