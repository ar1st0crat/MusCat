import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Performer, Country } from '../performer/performer.component';
import { PerformerService } from 'src/app/services/performer.service';

@Component({
  selector: 'app-performer-dialog',
  templateUrl: 'performer-dialog.component.html',
  styleUrls: ['performer-dialog.component.css']
})
export class PerformerDialog {

  countries: Country[];
  selectedCountryId: number;

  constructor(
    public dataService: PerformerService,
    public dialogRef: MatDialogRef<PerformerDialog>,
    @Inject(MAT_DIALOG_DATA) public data: Performer) {
      this.selectedCountryId = this.data.Country ? this.data.Country.Id : undefined;
      this.dataService.getCountries().subscribe(c => this.countries = c);
    }

  onSave(): void {
    const idx = this.countries.findIndex(c => c.Id === this.selectedCountryId);
    this.data.Country = this.countries[idx];
    this.dialogRef.close(this.data);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
