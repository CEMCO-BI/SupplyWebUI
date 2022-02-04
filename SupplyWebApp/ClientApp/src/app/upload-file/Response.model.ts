export class Response{
    public successfull:boolean;
    public message:string;

    constructor(successfull:boolean, message:string){
        this.successfull = successfull;
        this.message = message;
    }
}