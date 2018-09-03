$(document).ready(function(){
	
	$(".nav > li").click(function(e) {
		var me = this;
		$(me).find("ul").slideToggle('slow');
		var hasUp = $(me).find(".title-toggle > span").hasClass('glyphicon-menu-up');
		if(hasUp) {
			$(me).find(".title-toggle > span").removeClass('glyphicon-menu-up').addClass('glyphicon-menu-down');
		} else {
			$(me).find(".title-toggle > span").removeClass('glyphicon-menu-down').addClass('glyphicon-menu-up');
		}
	});
	
	$(".nav > li > ul").click(function(e) {
		e.stopPropagation();
	})
	
	$(".nav-toggle").click(function(e) {
		$(".flex-nav").fadeToggle('slow', 'linear');
	});
	
});