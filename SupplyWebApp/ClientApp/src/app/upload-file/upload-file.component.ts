import { HttpClient, HttpRequest, HttpEventType, HttpResponse, HttpHeaders } from '@angular/common/http'
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
  data: [][];
  baseUrl: string;
  @ViewChild('labelImport', { static: true })
  @ViewChild('file', { static: false })
  InputVar: ElementRef;

  //type of file
  typeof: string = 'default';

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private toastr: ToastrService) {
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    
  };

  reset() {
    // We will clear the value of the input 
    // field using the reference variable.

    this.InputVar.nativeElement.value = "";
    this.data = [[], []];
  }

  upload(files) {
    if (files.length === 0) {
      this.toastr.error('please select a file to upload');
      return;
    }
    const formData = new FormData();

    for (const file of files) {
      formData.append(file.name, file);
      formData.append('typeof',this.typeof);
    }

    const uploadReq = new HttpRequest('POST', this.baseUrl + 'FileUpload/upload', formData, {
      reportProgress: true
    });

    this.http.request(uploadReq).subscribe(event => {
      
    });  }

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

      console.log(ws);

      this.data = (XLSX.utils.sheet_to_json(ws, { header: 1 }));

      console.log(this.data);

      let x = this.data.slice(1);
      console.log(x);

    };

    reader.readAsBinaryString(target.files[0]);

  }
}
