﻿using FreeVideo.Models;
using SQLite;

namespace FreeVideo.Data
{
    public class VideoDatabase
    {
        SQLiteAsyncConnection Database;

        public VideoDatabase()
        {

        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            await Database.CreateTableAsync<VideoHistoryModel>();
            await Database.CreateTableAsync<VideoPlayListModel>();
        }

        public async Task<List<VideoHistoryModel>> GetHisVideoListAsync()
        {
            await Init();
            return await Database.Table<VideoHistoryModel>().OrderByDescending(it => it.show_time).ToListAsync();
        }


        public async Task<VideoHistoryModel> GetHisVideoAsync(string vod_id)
        {
            await Init();
            return await Database.Table<VideoHistoryModel>().Where(i => i.vod_id == vod_id).FirstOrDefaultAsync();
        }

        public async Task<List<VideoPlayListModel>> GetVideoPlayUrlAsync(string vod_id)
        {
            await Init();
            return await Database.Table<VideoPlayListModel>().Where(i => i.vod_id == vod_id).ToListAsync();
        }

        public async Task<int> SaveHisVideoAsync(VideoHistoryModel item)
        {
            await Init();
            var old = await Database.Table<VideoHistoryModel>().Where(i => i.vod_id == item.vod_id).FirstOrDefaultAsync();
            if (old != null)
                return await Database.UpdateAsync(item);
            else
                return await Database.InsertAsync(item);
        }

        public async Task<int> SaveVideoPlayUrlAsync(string vod_id, List<VideoPlayListModel> list)
        {
            await Init();
            await Database.Table<VideoPlayListModel>().DeleteAsync(i => i.vod_id == vod_id);

            return await Database.InsertAllAsync(list);
        }

        public async Task<int> SaveVideoPlayUrlAsync(VideoPlayListModel item)
        {
            await Init();
            return await Database.UpdateAsync(item);
        }

        public async Task<int> DeleteHisVideoAsync(VideoHistoryModel item)
        {
            await Init();
            return await Database.DeleteAsync(item);
        }
    }
}
