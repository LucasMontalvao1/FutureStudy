import { Component, OnInit, Input, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent implements OnInit {
  @Input() appName: string = 'FutureStudy';
  isMenuOpen: boolean = false;
  isMobileView: boolean = false;
  isScrolled: boolean = false;
  notifications: number = 5; // Número de exemplo para notificações
  userData: any;
  cadastroMenuOpen: boolean = false;
  
  constructor(
    private router: Router,
    private authService: AuthService,
    private elementRef: ElementRef
  ) { }
  
  ngOnInit(): void {
    this.checkScreenSize();
    
    // Obter dados do usuário do token
    if (this.authService.isAuthenticated()) {
      this.userData = this.authService.getDecodedToken();
    }
  }
  
  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 20;
  }
  
  @HostListener('window:resize', [])
  onResize() {
    this.checkScreenSize();
  }
  
  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent) {
    // Fechar dropdown ao clicar fora
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.cadastroMenuOpen = false;
    }
  }
  
  checkScreenSize() {
    this.isMobileView = window.innerWidth < 992;
    
    // Se a tela for redimensionada para uma largura maior, feche o menu mobile
    if (!this.isMobileView) {
      this.isMenuOpen = false;
    }
  }
  
  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
    
    // Bloquear o scroll do body quando o menu está aberto em dispositivos móveis
    if (this.isMenuOpen) {
      document.body.classList.add('menu-open');
      this.cadastroMenuOpen = false;
    } else {
      document.body.classList.remove('menu-open');
    }
  }
  
  toggleCadastroMenu(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.cadastroMenuOpen = !this.cadastroMenuOpen;
  }
  
  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}