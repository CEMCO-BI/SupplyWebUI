import { HttpClient, HttpRequest, HttpEventType, HttpResponse, HttpHeaders, HttpParams } from '@angular/common/http'
import { Component, ElementRef, Inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import * as XLSX from 'xlsx';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-upload-file',
  templateUrl: './upload-file.component.html',
  styles: []
})

export class UploadFileComponent implements OnInit {
  sheet: [][];
  header: [][];
  data: [][];
  x: [][];
  baseUrl: string;
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
   this.display = !this.display;
   
 }
  reset() {
    // We will clear the value of the input 
    // field using the reference variable.

    this.InputVar.nativeElement.value = "";
    this.data = [[], []];
    this.header = [[], []];
  }

  upload(files) {
    
    console.log(this.from);
    console.log(this.to);


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
      console.log(event);
      if (event instanceof HttpResponse) {
        if (event.status == 200)
          this.toastr.warning("", " Uploading ...", { positionClass: 'toast-bottom-center', progressBar: true, timeOut: 2000, progressAnimation: 'increasing' });
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
    let name = target.files[0].name;
    let allowedExtensions = /(\.xls|\.xlsx)$/i;
    if (!allowedExtensions.exec(name)) {
      //alert('Please select an Excel File');
      this.toastr.error('Please select an Excel File');
      file.value = '';
      //return false;
    }
    const reader: FileReader = new FileReader();

    reader.onload = (e: any) => {
      const bstr: string = e.target.result;

      const wb: XLSX.WorkBook = XLSX.read(bstr, { type: 'binary' });

      const wsname: string = wb.SheetNames[0];

      const ws: XLSX.WorkSheet = wb.Sheets[wsname];

      this.sheet = (XLSX.utils.sheet_to_json(ws, { header: 1 }));
      var headerEnd = this.typeOfFile == "F_02" ? 8 : 1

      this.data = this.sheet.slice(headerEnd);
      this.header = this.sheet.slice(0, headerEnd);
      //console.log(x);

    };

    reader.readAsBinaryString(target.files[0]);

  }
}
