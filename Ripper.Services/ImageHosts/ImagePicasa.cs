﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImagePicasa.cs" company="The Watcher">
//   Copyright (c) The Watcher Partial Rights Reserved.
//  This software is licensed under the MIT license. See license.txt for details.
// </copyright>
// <summary>
//   Code Named: VG-Ripper
//   Function  : Extracts Images posted on RiP forums and attempts to fetch them to disk.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Ripper.Services.ImageHosts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Net;
    using System.Threading;

    using Ripper.Core.Components;
    using Ripper.Core.Objects;

    /// <summary>
    /// Worker class to get images from ImagePicasa.com
    /// </summary>
    public class ImagePicasa : ServiceTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePicasa" /> class.
        /// </summary>
        /// <param name="savePath">The save Path.</param>
        /// <param name="imageUrl">The image Url.</param>
        /// <param name="thumbUrl">The thumb URL.</param>
        /// <param name="imageName">Name of the image.</param>
        /// <param name="hashtable">The hash table.</param>
        public ImagePicasa(ref string savePath, ref string imageUrl, ref string thumbUrl, ref string imageName, ref int imageNumber, ref Hashtable hashtable)
            : base(savePath, imageUrl, thumbUrl, imageName, imageNumber, ref hashtable)
        {
        }

        /// <summary>
        /// Do the Download
        /// </summary>
        /// <returns>
        /// Returns if the Image was downloaded
        /// </returns>
        protected override bool DoDownload()
        {
            var imageURL = this.ImageLinkURL;
            var thumbURL = this.ThumbImageURL;

            if (this.EventTable.ContainsKey(imageURL))
            {
                return true;
            }

            var filePath = string.Empty;

            try
            {
                if (!Directory.Exists(this.SavePath))
                {
                    Directory.CreateDirectory(this.SavePath);
                }
            }
            catch (IOException ex)
            {
                // MainForm.DeleteMessage = ex.Message;
                // MainForm.Delete = true;
                return false;
            }

            var cacheObject = new CacheObject { IsDownloaded = false, FilePath = filePath, Url = imageURL };

            try
            {
                this.EventTable.Add(imageURL, cacheObject);
            }
            catch (ThreadAbortException)
            {
                return true;
            }
            catch (Exception)
            {
                if (this.EventTable.ContainsKey(imageURL))
                {
                    return false;
                }

                this.EventTable.Add(imageURL, cacheObject);
            }

            // Set the download Path
            var imageDownloadURL = thumbURL.Replace(@"/upload/small/", @"/upload/big/");

            // Set Image Name instead of using random name
            filePath = this.GetImageName(this.PostTitle, imageDownloadURL, this.ImageNumber);

            filePath = Path.Combine(this.SavePath, Utility.RemoveIllegalCharecters(filePath));

            //////////////////////////////////////////////////////////////////////////
            var newAlteredPath = Utility.GetSuitableName(filePath);
            if (filePath != newAlteredPath)
            {
                filePath = newAlteredPath;
                ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = filePath;
            }

            try
            {
                var client = new WebClient();
                client.Headers.Add($"Referer: {imageURL}");
                client.Headers.Add("User-Agent: Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US; rv:1.7.10) Gecko/20050716 Firefox/1.0.6");
                client.DownloadFile(imageDownloadURL, filePath);
                client.Dispose();
            }
            catch (ThreadAbortException)
            {
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return true;
            }
            catch (IOException ex)
            {
                // MainForm.DeleteMessage = ex.Message;
                // MainForm.Delete = true;
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return true;
            }
            catch (WebException)
            {
                ((CacheObject)this.EventTable[imageURL]).IsDownloaded = false;
                ThreadManager.GetInstance().RemoveThreadbyId(this.ImageLinkURL);

                return false;
            }

            ((CacheObject)this.EventTable[this.ImageLinkURL]).IsDownloaded = true;
            CacheController.Instance().LastPic = ((CacheObject)this.EventTable[this.ImageLinkURL]).FilePath = filePath;

            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        
    }
}