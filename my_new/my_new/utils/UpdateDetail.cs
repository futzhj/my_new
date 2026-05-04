namespace my_new.utils;

internal struct UpdateDetail
{
	public string status;

	public string filename;

	public int speed;

	public int estimated_time;

	public int total_size;

	public int remain_size;

	public int progress;

	public UpdateDetail(string status, string filename, int speed, int estimated_time, int total_size, int remain_size, int progress)
	{
		this.status = status;
		this.filename = filename;
		this.speed = speed;
		this.estimated_time = estimated_time;
		this.total_size = total_size;
		this.remain_size = remain_size;
		this.progress = progress;
	}

	public UpdateDetail(string filename)
	{
		status = "prepare";
		this.filename = filename;
		speed = 0;
		estimated_time = 0;
		total_size = 0;
		remain_size = 0;
		progress = 0;
	}
}
