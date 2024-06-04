declare global {
  interface Navigator {
    msSaveBlob?: (blob: any, defaultName?: string) => boolean;
  }
}

export const startDownload = (
  blob: Blob,
  fileName: string = "downloaded-file",
) => {
  try {
    if (navigator.msSaveBlob) {
      navigator.msSaveBlob(blob, fileName);
    } else {
      // create a temporary anchor on document and remove it after download is started
      const blobUrl = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = blobUrl;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(blobUrl);
    }
  } catch (error) {
    console.error("Error fetching the file:", error);
  }
};

export const arrayToCsv = (data: string[][]) => {
  return data
    .map(
      (row) =>
        row
          .map((v) => v.replaceAll('"', '""')) // escape double quotes
          .map((v) => (v.search(/("|,|\n)/g) >= 0 ? `"${v}"` : v)) // quote it
          .join(","), // comma-separated
    )
    .join("\r\n"); // ro
};
