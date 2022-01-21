import { HttpClient, HttpRequest, HttpEventType, HttpResponse, HttpHeaders, HttpParams } from '@angular/common/http'
import { Component, ElementRef, Inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import * as XLSX from 'xlsx';
import { ToastrService } from 'ngx-toastr';
import { GlobalConstants } from '../common/global-constant';

@Component({
  selector: 'app-upload-file',
  templateUrl: './upload-file.component.html',
  styleUrls: ['./upload-file.component.css']
})

export class UploadFileComponent implements OnInit {
  sheet: [][];
  header: [][];
  data: [][];
  x: [][];
  baseUrl: string;
  fileName: string;
  @ViewChild('labelImport', { static: true })
  @ViewChild('file', { static: false })
  InputVar: ElementRef;

  //type of file
  typeOfFile: string = "F_01";
  display: boolean = false;
  to: string = '2022-01-01'; 
  from: string = '2022-01-01';
  
  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private toastr: ToastrService) {
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    
  };


 openDatePicker() {
   //this.display = true;
   this.display = true;
   this.InputVar.nativeElement.value = "";
    this.data = [[], []];
    this.header = [[], []];
  }

  reset() {
    // We will clear the value of the input 
    // field using the reference variable.
    this.display = false
    this.InputVar.nativeElement.value = "";
    this.data = [[], []];
    this.header = [[], []];
  }

  upload(files) {
    
    if (files.length === 0) {
      this.toastr.error('Please select a file to upload.');
      return;
    }
    const formData = new FormData();

    for (const file of files) {
      formData.append(file.name, file);
    }


    const uploadReq = new HttpRequest('POST', this.baseUrl + 'FileUpload/upload', formData, {
      reportProgress: true,
      params: new HttpParams().set('typeOfFile', this.typeOfFile).set('from',this.from).set('to',this.to)
    });

    this.http.request(uploadReq).subscribe(event => {
      if (event instanceof HttpResponse) {
        if (event.status == 200)
          this.toastr.info("", " Uploading ...", { positionClass: 'toast-bottom-center', progressBar: true, timeOut: 2000, progressAnimation: 'increasing' });
        setTimeout(() => {
          this.toastr.success("", " Uploaded successfully", { positionClass: 'toast-bottom-center', timeOut: 1000, progressBar: false })
          this.reset();
        }, 2500);
      }
    });
  }

  onFileChange(evt: any, file) {
    
    const target: DataTransfer = <DataTransfer>(evt.target);
 
    if (target.files.length !== 1) {
      this.toastr.error('Cannot upload multiple files');
    }
    this.fileName = target.files[0].name;
    let allowedExtensions = /(\.xls|\.xlsx)$/i;
    if (!allowedExtensions.exec(this.fileName)) {
      this.toastr.error('Please select an Excel File');
      file.value = '';
    }
    const reader: FileReader = new FileReader();

    reader.onload = (e: any) => {
      let isValidFile: boolean
      
      const bstr: string = e.target.result;

      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      const wsname: string = wb.SheetNames[0];

      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      this.sheet = (XLSX.utils.sheet_to_json(ws, { header: 1, raw:false }));
      var headerEnd = this.typeOfFile == GlobalConstants.F_02 ? 8 : 1

      this.data = this.sheet.slice(headerEnd);
      this.header = this.sheet.slice(0, headerEnd);

      isValidFile = this.checkFileValidation();
      if (!isValidFile) {
        this.toastr.error('Their is column mismatch in selected file. Please select appropriate file for File Type');
        this.reset();
      }
    };

    reader.readAsBinaryString(target.files[0]);
  }

  checkFileValidation(): boolean {
    let isValidFile: boolean
    let fileColumnHeader: string[][] = [['Year', 'Month', 'Location', 'Amount'], ['Spot prices', 'WEEK 1', 'WEEK 2', 'WEEK 3', 'WEEK 4', 'WEEK 5'], ['Year', 'Month', 'Location', 'Amount']];

    if (this.typeOfFile == GlobalConstants.F_01 && this.fileName.indexOf(GlobalConstants.SalesForecast) > 0) {
      for (var i = 0; i < 3; i++) {
        if (this.header[0][i] == fileColumnHeader[0][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    else if (this.typeOfFile == GlobalConstants.F_02 && this.fileName.indexOf(GlobalConstants.CRUPricing) > 0) {
      for (var i = 0; i < 5; i++) {
        if (this.header[0][i] == fileColumnHeader[1][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    else if (this.typeOfFile == GlobalConstants.F_03 && this.fileName.indexOf(GlobalConstants.PlannedBuy) > 0) {
      for (var i = 0; i < 3; i++) {
        if (this.header[0][i] == fileColumnHeader[2][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    return isValidFile;
  }
}
