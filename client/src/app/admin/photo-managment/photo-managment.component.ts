import { Component, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-managment',
  templateUrl: './photo-managment.component.html',
  styleUrls: ['./photo-managment.component.css']
})
export class PhotoManagmentComponent implements OnInit {
  photos: Photo[] = [];
  constructor(private AdminServices: AdminService) {
    
  }
  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval(){
    this.AdminServices.getPhotosForApproval().subscribe({
      next: photos => this.photos = photos
    })
  }

  approvePhoto(photoId: number){
    this.AdminServices.approvePhoto(photoId).subscribe({
      next: () => this.photos.splice(this.photos.findIndex(p=>p.id === photoId),1)
    })
  }

  rejectPhoto(photoId: number){
    this.AdminServices.rejectPhoto(photoId).subscribe({
      next: () => this.photos.splice(this.photos.findIndex(p=>p.id === photoId),1)
    })
  }
}
