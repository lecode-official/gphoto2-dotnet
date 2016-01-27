# Trouble Shooting

If you run into problems, i.e. `CameraException` is thrown, although the camera is properly attached to your system, this might be due to the
`gvfs-gphoto2-volume-monitor`, which comes pre-installed with several Linux distributions, including Raspbian. The `gvfs-gphoto2-volume-monitor`
captures the USB port that the camera is attached to and therefore gPhoto2.NET can not access the camera. You'll have to kill the
`gvfs-gphoto2-volume-monitor` process before you are able to use gPhoto2.NET. To find out if `gvfs-gphoto2-volume-monitor` is blocking you from
accessing the camera you can use `ps`:

```bash
user@machine ~ $ ps -aux | grep gphoto
user     1901  0.0  0.1 210060  6140 ?        Sl   21:06   0:00 /usr/lib/gvfs/gvfs-gphoto2-volume-monitor
user     2350  0.0  0.0  13260  2152 pts/2    S+   21:10   0:00 grep --colour=auto gphoto
```

You can kill the process by calling:

```bash
user@machine ~ $ kill -SIGTERM 1901
```

Please make sure to replace the process ID with the process ID that you got from `ps`.