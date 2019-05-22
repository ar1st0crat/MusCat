import { Component, Input, OnInit } from '@angular/core';
import { PerformerService } from 'src/app/services/performer.service';
import { Performer } from '../performer/performer.component';

export interface Album {
  Id: number;
  Name: string;
  ReleaseYear: number;
  TotalTime: string;
  Rate: number;
  Link: string;
}

export interface Song {
  Id: number;
  TrackNo: number;
  Name: string;
  TimeLength: string;
}

@Component({
  selector: 'app-albums-panel',
  templateUrl: './albums-panel.component.html',
  styleUrls: ['./albums-panel.component.css']
})
export class AlbumsPanelComponent implements OnInit {

  @Input() albums: Album[];
  @Input() album: Album;
  @Input() performer: Performer;

  songs: Song[];

  constructor(private performerService: PerformerService) { }

  ngOnInit() {
  }

  selectAlbum(album: Album) {
    this.album = album;
    this.performerService.getSongs(album.Id)
      .subscribe(a => { this.songs = a; });
  }
}
