deps:
	git submodule init
	git submodule update
	cd submodules/xr-http-json && xbuild JsonRpcHost.sln
	cd submodules/xr-include &&	xbuild xr-include.sln
