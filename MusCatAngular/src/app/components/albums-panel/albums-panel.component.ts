import { Component, Input, OnInit } from '@angular/core';

export interface Album {
  Id: number;
  Name: string;
  ReleaseYear: number;
  TimeLength: string;
  Rate: number;
  Link: string;
}

@Component({
  selector: 'app-albums-panel',
  templateUrl: './albums-panel.component.html',
  styleUrls: ['./albums-panel.component.css']
})
export class AlbumsPanelComponent implements OnInit {

  @Input() albums: Album[];

  constructor() { }

  ngOnInit() {
  }

}
