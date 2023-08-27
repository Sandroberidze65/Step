import { Photo } from "./photo"

export interface Member {
  id: number
  userName: string
  photoUrl: string
  age: number
  knownAs: string
  created: Date
  lastActive: Date
  introduction: string
  gender: string
  lookingFor: string
  interests: string
  city: string
  country: string
  photos: Photo[]
}


