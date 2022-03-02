import { HttpClient, HttpRequest, HttpEventType, HttpResponse, HttpHeaders, HttpParams } from '@angular/common/http'
import { Component, ElementRef, Inject, Input, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import * as XLSX from 'xlsx';
import { Toast, ToastrService } from 'ngx-toastr';
import { GlobalConstants } from '../common/global-constant';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-upload-file',
  templateUrl: './upload-file.component.html',
  styleUrls: ['./upload-file.component.css']
})

export class UploadFileComponent implements OnInit {
  errorlist: {
    LineNumber: number, ErrorMessage: string, RowData: { Id: number, Year: number, Month: number, Location: string, Amount: number }
  }[];
  message: string;
  sheet: [][];
  header: [][];
  data: [][];
  x: [][];
  baseUrl: string;
  fileName: string;
  @ViewChild('labelImport', { static: true })
  @ViewChild('file', { static: false })
  InputVar: ElementRef;
  displayerrors: boolean = false;

  //type of file
  typeOfFile: string = "F_01";
  typo: string = "F_01";
  display: boolean = false;
  displayGrid: boolean = false;
  to: null;
  from: null;
  

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string, private toastr: ToastrService, private route: ActivatedRoute) {
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    

    if (this.route.snapshot.params.typo == 'F_02' || this.route.snapshot.params.typo == 'F_03' || this.route.snapshot.params.typo == 'F_01') {
      this.typeOfFile = this.route.snapshot.params.typo;
      
    }
    if (this.typeOfFile == 'F_02') {
      this.display = true;
    }
    
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
    document.getElementById("btnUpload").blur();
    document.getElementById("btnReset").blur();
    
    this.display = false;
    this.displayGrid = false;
    this.displayerrors = false;
    this.InputVar.nativeElement.value = "";
    this.data = [[], []];
    this.header = [[], []];

    if(this.typeOfFile == 'F_02'){
      this.openDatePicker();
    }
  }

  errorDataOrTableData() {
    if (this.errorlist.length) {
      this.displayerrors = true;
      this.displayGrid = false;

    } 
  }

 
  upload(files) {
    
    if (files.length === 0) {
      this.toastr.error('Please select a file to upload.');
      return;
    }
    if (this.typeOfFile == 'F_02' && (this.to == null || this.from == null)) {
      this.toastr.error('Please select FROM and TO date');
      document.getElementById("fromdate").focus();
      document.getElementById("todate").focus();
      return;
    }
    const formData = new FormData();

    for (const file of files) {
      formData.append(file.name, file);
    }

    
    const uploadReq = new HttpRequest('POST', this.baseUrl + 'FileUpload/upload', formData, {
      reportProgress: true,
      params: new HttpParams().set('typeOfFile', this.typeOfFile).set('from', this.from).set('to', this.to)
    });
    //check
    
    this.toastr.info("Please wait while your file is being uploaded.", " Upload in Progress...", { positionClass: 'toast-top-center', progressBar: false, progressAnimation: 'increasing' });
    this.http.request(uploadReq).subscribe(event => {
      
      if (event instanceof HttpResponse) {
        var response = event.body;
        console.log(event.body);
        console.log(response['Successful'])
        this.errorlist = response['ErrorList'];
        this.errorDataOrTableData() 
       

        if (response['Successful']) {
         
            this.toastr.clear();
            this.toastr.success("Your file has been uploaded successfully.", " Upload Successful...", { positionClass: 'toast-top-center', timeOut: 3000, progressBar: false })
          this.reset();
          
          
        } else {
          console.log('there are errrors')
          this.toastr.clear();
            this.toastr.error("There are some errors in the excel file.","Upload Failed...", { timeOut: 5000, progressBar: false });
         
        }
      }
    });
    document.getElementById("btnUpload").blur();
    document.getElementById("btnReset").blur();
  }

  onFileChange(evt: any, file) {

    this.displayGrid = true;
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

      this.sheet = (XLSX.utils.sheet_to_json(ws, { header: 1, raw: false }));
      var headerEnd = this.typeOfFile == GlobalConstants.F_02 ? 8 : 1

      this.data = this.sheet.slice(headerEnd);
      this.header = this.sheet.slice(0, headerEnd);

      isValidFile = this.checkFileValidation();
      if (!isValidFile) {
        this.toastr.error('Columns do not match. Please select appropriate file for File Type');
        this.reset();
      }
    };

    reader.readAsBinaryString(target.files[0]);

  }

  checkFileValidation(): boolean {
    let isValidFile: boolean
    let fileColumnHeader: string[][] = [['Year', 'Month', 'Location', 'Amount'], ['Spot prices', 'WEEK 1', 'WEEK 2', 'WEEK 3', 'WEEK 4', 'WEEK 5'], ['Year', 'Month', 'Location', 'Amount','CWT']];

    if (this.typeOfFile == GlobalConstants.F_01) {
      for (var i = 0; i < 3; i++) {
        if (this.header[0][i] == fileColumnHeader[0][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    else if (this.typeOfFile == GlobalConstants.F_02) {
      for (var i = 0; i < 5; i++) {
        if (this.header[0][i] == fileColumnHeader[1][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    else if (this.typeOfFile == GlobalConstants.F_03) {
      for (var i = 0; i < 5; i++) {
        if (this.header[0][i] == fileColumnHeader[2][i])
          isValidFile = true;
        else
          isValidFile = false
      }
    }
    return isValidFile;
  }
}
