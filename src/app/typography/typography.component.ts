import { Component, Input, OnInit } from '@angular/core';
import { FileDetails } from 'app/file.model';
import { FileService } from 'app/services/file.service';

@Component({
  selector: 'app-typography',
  templateUrl: './typography.component.html',
  styleUrls: ['./typography.component.css']
})
export class TypographyComponent implements OnInit {

  graphValue!: string
  graphValueChange: boolean = false
  fileObjects: FileDetails[] = []
  actionObjects: FileDetails[] = []
  constructor(private fileService: FileService) { }

  // onSelectChange() {
  //   console.log("in onselectchange");
  //   console.log(this.graphValueChange)
  //   return this.graphValueChange = !this.graphValueChange;
  // }
  ngOnInit() {
    this.fileObjects = this.fileService.getFileObjects();
    console.log(this.fileObjects);
    this.actionObjects = this.fileService.getActionObjects();
  }

}
