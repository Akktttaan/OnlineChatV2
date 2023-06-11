export function fileToBase64(file: File): Promise<string> {
  return new Promise<string>((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = (event: any) => {
      const base64String = event.target.result.split(',')[1];
      resolve(base64String);
    };

    reader.onerror = (event: any) => {
      reject(event.target.error);
    };

    reader.readAsDataURL(file);
  });
}
